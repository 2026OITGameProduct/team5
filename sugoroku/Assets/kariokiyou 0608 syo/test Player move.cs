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
    private bool isForcedMoving = false; 

    private Vector3 playerOffset; 
    private EventPopupManager popupManager; 
    private CanvasGroup diceCanvasGroup;

    // 外部のMasuEventからポイントを直接書き換えられるようにしています
    public int score { get; set; } = 0;
    public int lapCount { get; private set; } = 0;

    // 1回休み状態を記録するフラグ
    public bool isSkippingNextTurn { get; set; } = false;

    private void Awake()
    {
        popupManager = Object.FindAnyObjectByType<EventPopupManager>();

        // サイコロのオブジェクトを自動検索してロック用の部品を準備
        GameObject diceObj = GameObject.Find("DiceManager") ?? GameObject.Find("Dice") ?? GameObject.Find("RollButton");
        if (diceObj != null)
        {
            diceCanvasGroup = diceObj.GetComponent<CanvasGroup>();
            if (diceCanvasGroup == null)
            {
                diceCanvasGroup = diceObj.AddComponent<CanvasGroup>();
            }
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

    // 外部からポイントが変わった時にもこれを呼び出して画面を更新します
    public void UpdateUI()
    {
        if (scoreText != null) scoreText.text = $"ポイント: {score} pt";
        if (lapText != null) lapText.text = $"{lapCount} 周目";
    }

    public void MoveSteps(int steps)
    {
        if (isGoal) return;
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
            StartCoroutine(MoveBackRoutine(steps));
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

        // 効果移動で着地した場合は、そこで追加のイベントポップアップを出さない（無限ループ防止）
        if (isForcedMoving)
        {
            OnLandOnWaypoint(currentWaypointIndex);
            isForcedMoving = false; 
            yield break; 
        }

        GameObject currentMasuObject = routeWaypoints[currentWaypointIndex].gameObject;
        MasuEvent masuEvent = currentMasuObject.GetComponentInChildren<MasuEvent>();

        if (popupManager != null)
        {
            Sprite imageToShow = (masuEvent != null) ? masuEvent.eventImage : null;

            // ポップアップが出るのでサイコロをロック
            SetDiceInteraction(false);

            popupManager.ShowEventPopup(imageToShow, () =>
            {
                // OKボタンが押されたらサイコロのロックを解除
                SetDiceInteraction(true);

                // マスの基本的な着地処理
                OnLandOnWaypoint(currentWaypointIndex);

                // マスに設定された個別イベント（3マス進む、ポイント増減、1回休み等）を実行
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

    // 逆走アニメーション用の処理
    private IEnumerator MoveBackRoutine(int steps)
    {
        isMoving = true;

        for (int i = 0; i < steps; i++)
        {
            int prevIndex = currentWaypointIndex - 1;
            if (prevIndex < 0)
            {
                prevIndex = routeWaypoints.Length - 1;
            }

            currentWaypointIndex = prevIndex;
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
        OnLandOnWaypoint(currentWaypointIndex);
        isForcedMoving = false;
    }

    private void SetDiceInteraction(bool interactable)
    {
        if (diceCanvasGroup != null)
        {
            diceCanvasGroup.blocksRaycasts = interactable; 
            diceCanvasGroup.alpha = interactable ? 1.0f : 0.6f; 
        }
    }

    private void OnLapPassed()
    {
        lapCount++;
        score += 1; // 周回ボーナス
        UpdateUI(); 
    }

    private void OnLandOnWaypoint(int waypointIndex)
    {
        if (logText != null) logText.text = $"{waypointIndex + 1}番目のマスに着地。";

        if (waypointIndex == 0 && score >= 3)
        {
            isGoal = true;
        }

        UpdateUI(); 
    }
}