using System.Collections;
using TMPro;
using UnityEngine;

public class LoopSugorokuPlayer : MonoBehaviour
{
    [SerializeField] private Transform[] routeWaypoints;
    [SerializeField] private float moveSpeed = 5f;

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI lapText;
    [SerializeField] private TextMeshProUGUI logText;

    [Header("移動音の設定")]
    [SerializeField] private AudioClip moveSound;
    [SerializeField] private AudioSource audioSource;

    private int currentWaypointIndex = 0;
    private bool isMoving = false;
    private bool isGoal = false; 

    private bool isForcedMoving = false;

    public int score { get; set; } = 0;
    public int lapCount { get; private set; } = 0;

    public bool isSkippingNextTurn { get; set; } = false;

    private Vector3 playerOffset;
    private EventPopupManager popupManager;

    // 🔴 連続イベント中（1枚目〜2枚目が終わるまで）であることを記録するフラグ
    private bool isProcessingComboEvent = false;

    private void Awake()
    {
        popupManager = Object.FindAnyObjectByType<EventPopupManager>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void SetupPlayer(Transform[] waypoints, int id, TMPro.TextMeshProUGUI sText, TMPro.TextMeshProUGUI lText, TMPro.TextMeshProUGUI logTxt)
    {
        this.routeWaypoints = waypoints;
        this.scoreText = sText;
        this.lapText = lText;
        this.logText = logTxt;
        this.playerOffset = new Vector3((id % 2) * 0.4f, 1f, (id / 2) * 0.4f);

        currentWaypointIndex = 0;
        if (routeWaypoints != null && routeWaypoints.Length > 0)
        {
            transform.position = routeWaypoints[0].position + playerOffset;
        }
        UpdateUI();
    }

    private void Start()
    {
        UpdateUI();
        if (logText != null) logText.text = "ゲームスタート！ダイスを振ってください。";
    }

    public void UpdateUI()
    {
        if (scoreText != null) scoreText.text = $"ポイント: {score} pt";
        if (lapText != null) lapText.text = $"{lapCount} 周目";
    }

    public void MoveSteps(int steps)
    {
        if (isGoal) return;

        if (isSkippingNextTurn)
        {
            isSkippingNextTurn = false; 
            if (logText != null) logText.text = $"{gameObject.name}は1回休みです！";
            return;
        }

        if (!isMoving)
        {
            isForcedMoving = false; 
            StartCoroutine(MoveRoutine(steps));
        }
    }

    public void MoveStepsByEvent(int steps)
    {
        if (isGoal) return;
        if (!isMoving)
        {
            isForcedMoving = true; 
            StartCoroutine(MoveRoutine(steps));
        }
    }

    public void MoveBackStepsByEvent(int steps)
    {
        if (isGoal) return;
        if (!isMoving)
        {
            isForcedMoving = true;
            StartCoroutine(MoveRoutine(-steps)); 
        }
    }

    private IEnumerator MoveRoutine(int steps)
    {
        isMoving = true;

        int direction = (steps > 0) ? 1 : -1;
        int absoluteSteps = Mathf.Abs(steps);
        bool passedStartThisTurn = false; 

        for (int i = 0; i < absoluteSteps; i++)
        {
            int nextIndex = (currentWaypointIndex + direction + routeWaypoints.Length) % routeWaypoints.Length;

            if (direction == 1 && nextIndex == 0)
            {
                OnLapPassed();

                if (i < absoluteSteps - 1)
                {
                    passedStartThisTurn = true;
                }
            }

            currentWaypointIndex = nextIndex;
            Vector3 targetPosition = routeWaypoints[currentWaypointIndex].position + playerOffset;

            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = targetPosition;

            if (audioSource != null && moveSound != null)
            {
                audioSource.PlayOneShot(moveSound);
                audioSource.PlayOneShot(moveSound);
            }

            yield return new WaitForSeconds(0.1f);
        }

        isMoving = false;

        if (isForcedMoving)
        {
            isForcedMoving = false;
            yield break; 
        }

        // ①【もし移動の途中でスタートマスを通過していた場合】
        if (passedStartThisTurn)
        {
            // 🔴 連続イベントが始まったのでフラグをONにする！（これでターン交代がロックされます）
            isProcessingComboEvent = true;

            GameObject startMasu = routeWaypoints[0].gameObject;
            MasuEvent startEvent = startMasu.GetComponentInChildren<MasuEvent>();

            if (startEvent != null && popupManager != null)
            {
                bool startPopupClosed = false; 

                popupManager.ShowEventPopup(startEvent.eventImage, startEvent.masuEventSound, () =>
                {
                    startEvent.OnPlayerStop(this);
                    startPopupClosed = true; 
                });

                yield return new WaitUntil(() => startPopupClosed);
                yield return new WaitForSeconds(0.6f);
            }
        }

        // ②【最後に、いま現在止まっているマスのイベントを実行】
        GameObject currentMasuObject = routeWaypoints[currentWaypointIndex].gameObject;
        MasuEvent masuEvent = currentMasuObject.GetComponentInChildren<MasuEvent>();

        if (popupManager != null)
        {
            bool endPopupClosed = false; 

            Sprite imageToShow = (masuEvent != null) ? masuEvent.eventImage : null;
            AudioClip soundToShow = (masuEvent != null) ? masuEvent.masuEventSound : null;

            popupManager.ShowEventPopup(imageToShow, soundToShow, () =>
            {
                if (masuEvent != null)
                {
                    masuEvent.OnPlayerStop(this);
                }
                endPopupClosed = true; 
            });

            // 2枚目のポップアップが完全に閉じられるのを待つ
            yield return new WaitUntil(() => endPopupClosed);

            // 🔴【ここが超重要！】
            // スタートを通過していた場合、ロックしていたターン交代処理をここで「今からやって！」と実行させます。
            if (passedStartThisTurn)
            {
                // ロックを解除
                isProcessingComboEvent = false;

                SugorokuManager sugorokuManager = Object.FindAnyObjectByType<SugorokuManager>();
                if (sugorokuManager != null)
                {
                    // 相手のシステム側で、本来ダイスを振った後にマスの効果が終わったタイミングで呼ばれる
                    // 「ターンを次に進める関数」を実行して、ここで初めて下の文字を「次のプレイヤー」に切り替えます。
                    sugorokuManager.OnDiceRolled(0); 
                }

                // ボタンの見た目も確実に「サイコロを振る」に戻す
                dicesystem dice = Object.FindAnyObjectByType<dicesystem>();
                if (dice != null)
                {
                    dice.EnableDiceButton(); 
                }
            }
        }
        else
        {
            if (masuEvent != null) masuEvent.OnPlayerStop(this);
            isProcessingComboEvent = false;
        }
    }

    private void OnLapPassed()
    {
        lapCount++;
        score += 1;
        UpdateUI();
    }

    // 🔴 dicesystemから「いまロック中？」と聞かれたら答える窓口
    public bool IsLockingTurn()
    {
        return isProcessingComboEvent;
    }
}