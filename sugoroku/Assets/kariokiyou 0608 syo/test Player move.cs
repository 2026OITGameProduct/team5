using System.Collections;
using TMPro;
using UnityEngine;
// シーンを切り替えるために必要なライブラリを追加
using UnityEngine.SceneManagement;

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

    // 連続イベント中（1枚目〜2枚目が終わるまで）であることを記録するフラグ
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

            // 前進中にスタートマス（インデックス0）をまたいだ瞬間（通過時）
            if (direction == 1 && nextIndex == 0)
            {
                OnLapPassed();

                // 【追加ルール：通過時チェック】
                // スタートを通過した時点で3pt以上持っていたら、即リザルト画面へ！
                if (score >= 3)
                {
                    SceneManager.LoadScene("ResultScene");
                    yield break; // 遷移するのでコルーチンを強制終了
                }

                if (i < absoluteSteps - 1)
                {
                    passedStartThisTurn = true;
                }
            }

            currentWaypointIndex = nextIndex;
            Vector3 targetPosition = routeWaypoints[currentWaypointIndex].position + playerOffset;

            // 🔴【合体機能！】次のマスの方角を計算して、動き出す直前にシュッと振り向かせる
            Vector3 directionVector = targetPosition - transform.position;
            directionVector.y = 0; // 首の上下の傾きは無視して、水平に回転させる

            if (directionVector != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionVector);

                // 0.1秒かけて滑らかに向き直る（テンポを損なわない絶妙な速度です）
                float rotationTimer = 0f;
                Quaternion startRotation = transform.rotation;
                while (rotationTimer < 0.1f)
                {
                    rotationTimer += Time.deltaTime;
                    transform.rotation = Quaternion.Slerp(startRotation, targetRotation, rotationTimer / 0.1f);
                    yield return null;
                }
                transform.rotation = targetRotation; // 最後に角度を完全にピッタリ合わせる
            }

            // 動き出す瞬間（タイミング前倒し）の移動音
            if (audioSource != null && moveSound != null)
            {
                audioSource.PlayOneShot(moveSound);
                audioSource.PlayOneShot(moveSound);
            }

            // 実際に次のマスへスーッと移動する
            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = targetPosition;

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

        // 【追加ルール：ぴったり停止時チェック】
        // 止まったマスがスタートマス（0番）で、かつ3pt以上持っていたら即リザルト画面へ！
        if (currentWaypointIndex == 0 && score >= 3)
        {
            SceneManager.LoadScene("ResultScene");
            yield break;
        }

        // スタートマスにぴったり止まった場合も連続イベントの起点としてロックをかける
        if (currentWaypointIndex == 0)
        {
            isProcessingComboEvent = true;
        }

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

            yield return new WaitUntil(() => endPopupClosed);

            // スタートを「通過」または「ぴったり停止」していた場合のアフターケア
            if (passedStartThisTurn || currentWaypointIndex == 0)
            {
                isProcessingComboEvent = false;

                SugorokuManager sugorokuManager = Object.FindAnyObjectByType<SugorokuManager>();
                if (sugorokuManager != null)
                {
                    sugorokuManager.OnDiceRolled(0);
                }

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
        score += 1; // 1周通過で+1pt（ここで3ptに達する可能性があります）
        UpdateUI();
    }

    public bool IsLockingTurn()
    {
        return isProcessingComboEvent;
    }
}