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

    private int currentWaypointIndex = 0;
    private bool isMoving = false;
    private bool isGoal = false;

    // 💡【新機能！】効果で動かされたかどうかを記録するフラグ
    private bool isForcedMoving = false; 

    private Vector3 playerOffset; 
    private EventPopupManager popupManager; 

    public int score { get; private set; } = 0;
    public int lapCount { get; private set; } = 0;

    private void Awake()
    {
        popupManager = Object.FindAnyObjectByType<EventPopupManager>();
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

    private void UpdateUI()
    {
        if (scoreText != null) scoreText.text = $"ポイント: {score} pt";
        if (lapText != null) lapText.text = $"{lapCount} 周目";
    }

    // 💡 通常のサイコロ移動（イベントを受け付ける）
    public void MoveSteps(int steps)
    {
        if (isGoal) return;
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

    private IEnumerator MoveRoutine(int steps)
    {
        isMoving = true;

        for (int i = 0; i < steps; i++)
        {
            int nextIndex = (currentWaypointIndex + 1) % routeWaypoints.Length;
            if (nextIndex == 0)
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
            yield return new WaitForSeconds(0.1f);
        }

        isMoving = false;

        // 💡【今回のメイン対策！】
        // もし「イベントの効果で移動してきた（isForcedMovingがtrue）」なら、ポップアップは出さずに素通りする
        if (isForcedMoving)
        {
            // 効果を受けずに、通常のプラス・マイナスの基礎計算だけして終了
            OnLandOnWaypoint(currentWaypointIndex);
            isForcedMoving = false; // 次のターンのためにフラグを元に戻しておく
            yield break; // ここで処理を終了（ポップアップを出さない）
        }

        // 💡 通常のサイコロ移動の時だけ、いつも通りポップアップを出す
        GameObject currentMasuObject = routeWaypoints[currentWaypointIndex].gameObject;
        MasuEvent masuEvent = currentMasuObject.GetComponentInChildren<MasuEvent>();

        if (popupManager != null)
        {
            Sprite imageToShow = (masuEvent != null) ? masuEvent.eventImage : null;

            popupManager.ShowEventPopup(imageToShow, () =>
            {
                OnLandOnWaypoint(currentWaypointIndex);

                if (masuEvent != null)
                {
                    masuEvent.OnPlayerStop(this);
                }
            });
        }
        else
        {
            OnLandOnWaypoint(currentWaypointIndex);
        }
    }

    private void OnLapPassed()
    {
        lapCount++;
        score += 1; 
        UpdateUI(); 
    }

    private void OnLandOnWaypoint(int waypointIndex)
    {
        if (waypointIndex == 3 || waypointIndex == 9)
        {
            score += 1;
            string msg = $"プラスマスに到着！ +1pt (合計:{score}pt)";
            if (logText != null) logText.text = msg;
        }
        else if (waypointIndex == 6)
        {
            score = Mathf.Max(0, score - 1);
            string msg = $"マイナスマスに到着... -1pt (合計:{score}pt)";
            if (logText != null) logText.text = msg;
        }

        if (waypointIndex == 0 && score >= 3)
        {
            isGoal = true;
        }

        UpdateUI(); 
    }
}