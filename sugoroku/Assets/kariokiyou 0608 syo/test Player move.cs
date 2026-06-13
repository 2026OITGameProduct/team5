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

    // 💡【新機能！】効果で動かされたかどうかを記録するフラグ
    private bool isForcedMoving = false;

    // 【メンバーのコードに対応！】 score を外部（MasuEvent）から直接書き換えられるように「set」を解放
    public int score { get; set; } = 0;
    public int lapCount { get; private set; } = 0;

    // 【メンバーのコードに対応！】 1回休みフラグの名前をメンバーの「isSkippingNextTurn」に統一
    public bool isSkippingNextTurn { get; set; } = false;

    private Vector3 playerOffset;
    private EventPopupManager popupManager;

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

    // 【メンバーのコードに対応！】 MasuEventから呼び出せるように「public」に変更
    public void UpdateUI()
    {
        if (scoreText != null) scoreText.text = $"ポイント: {score} pt";
        if (lapText != null) lapText.text = $"{lapCount} 周目";
    }

    // 💡 通常のサイコロ移動（イベントを受け付ける）
    public void MoveSteps(int steps)
    {
        if (isGoal) return;

        // 1回休みフラグ（メンバーの名前）が立っていたら、動かずに手番をスキップする
        if (isSkippingNextTurn)
        {
            isSkippingNextTurn = false; // フラグを戻して次のターンは動けるようにする
            if (logText != null) logText.text = $"{gameObject.name}は1回休みです！";
            return;
        }

        if (!isMoving)
        {
            isForcedMoving = false; // 自分のサイコロで動くのでフラグをリセット
            StartCoroutine(MoveRoutine(steps));
        }
    }

    // 💡【新機能！】イベントの効果で移動させるときはこっちを呼び出す
    public void MoveStepsByEvent(int steps)
    {
        if (isGoal) return;
        if (!isMoving)
        {
            isForcedMoving = true; // 「効果で動かされたよ！」という目印をつける
            StartCoroutine(MoveRoutine(steps));
        }
    }

    // 【メンバーのコードに対応！】 2マス戻る専用の命令を新設
    public void MoveBackStepsByEvent(int steps)
    {
        if (isGoal) return;
        if (!isMoving)
        {
            isForcedMoving = true;
            StartCoroutine(MoveRoutine(-steps)); // マイナスにして移動ルーチンに渡す
        }
    }

    private IEnumerator MoveRoutine(int steps)
    {
        isMoving = true;

        int direction = (steps > 0) ? 1 : -1;
        int absoluteSteps = Mathf.Abs(steps);

        for (int i = 0; i < absoluteSteps; i++)
        {
            int nextIndex = (currentWaypointIndex + direction + routeWaypoints.Length) % routeWaypoints.Length;

            if (direction == 1 && nextIndex == 0)
            {
                if (score >= 3)
                {
                    isGoal = true;
                    currentWaypointIndex = 0;
                    transform.position = routeWaypoints[0].position + playerOffset;
                    string clearMsg = "🎉 3ポイント持ってゴール通過！ゲームクリア！ 🎉";
                    if (logText != null) logText.text = clearMsg;
                    isMoving = false;
                    UpdateUI();
                    yield break;
                }
                OnLapPassed();
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
            yield break; // 効果移動時は進んだ先のマスの効果は発動しない
        }

        GameObject currentMasuObject = routeWaypoints[currentWaypointIndex].gameObject;
        MasuEvent masuEvent = currentMasuObject.GetComponentInChildren<MasuEvent>();

        if (popupManager != null)
        {
            Sprite imageToShow = (masuEvent != null) ? masuEvent.eventImage : null;
            // 🔴 【新設】マスに設定されている効果音を取得する（無ければnull）
            AudioClip soundToShow = (masuEvent != null) ? masuEvent.masuEventSound : null;

            // 🔴 【新設】新しく作った音源付きのShowEventPopupを呼び出す
            popupManager.ShowEventPopup(imageToShow, soundToShow, () =>
            {
                if (masuEvent != null)
                {
                    masuEvent.OnPlayerStop(this);
                }
            });
        }
        else
        {
            if (masuEvent != null) masuEvent.OnPlayerStop(this);
        }
    }

    private void OnLapPassed()
    {
        lapCount++;
        score += 1;
        UpdateUI();
    }
}