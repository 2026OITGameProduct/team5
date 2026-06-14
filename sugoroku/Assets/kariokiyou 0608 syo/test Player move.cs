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

    [Header("ゴール演出の設定")]
    [SerializeField] private GameObject confettiEffect;
    [SerializeField] private TextMeshProUGUI winTextUI;

    private int currentWaypointIndex = 0;
    private bool isMoving = false;
    private bool isGoal = false;

    private bool isForcedMoving = false;

    // 外部のdicesystemやMasuEventからも名前を読み取れるようにpublic（ゲットのみ）に変更
    public string playerName { get; private set; } = "";

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

    // 引数の最後に「string pName」を追加して、名前を受け取れるように拡張しました！
    public void SetupPlayer(Transform[] waypoints, int id, TextMeshProUGUI sText, TextMeshProUGUI lText, TextMeshProUGUI logTxt, string pName)
    {
        this.routeWaypoints = waypoints;
        this.scoreText = sText;
        this.lapText = lText;
        this.logText = logTxt;
        this.playerName = pName; // 名前を記憶！
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

        if (winTextUI != null) winTextUI.gameObject.SetActive(false);
        if (confettiEffect != null) confettiEffect.SetActive(false);
    }

    public void UpdateUI()
    {
        // 「プレイヤー名: 〇 pt」という表記に変更しました！
        // もし名前がまだ登録されていなければ、オブジェクト名（1Pなど）を代わりに使います
        string dispName = string.IsNullOrEmpty(playerName) ? gameObject.name : playerName;

        if (scoreText != null) scoreText.text = $"{dispName}: {score} pt";
        if (lapText != null) lapText.text = $"{lapCount} 周目";
    }

    public void MoveSteps(int steps)
    {
        if (isGoal) return;

        if (isSkippingNextTurn)
        {
            isSkippingNextTurn = false;
            string dispName = string.IsNullOrEmpty(playerName) ? gameObject.name : playerName;
            if (logText != null) logText.text = $"{dispName}は1回休みです！";
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

                if (score >= 3)
                {
                    yield return StartCoroutine(GoalPerformanceRoutine());
                    yield break;
                }

                if (i < absoluteSteps - 1)
                {
                    passedStartThisTurn = true;
                }
            }

            currentWaypointIndex = nextIndex;
            Vector3 targetPosition = routeWaypoints[currentWaypointIndex].position + playerOffset;

            Vector3 directionVector = targetPosition - transform.position;
            directionVector.y = 0;

            if (directionVector != Vector3.zero)
            {
                // 🔄 向きを変える演出
                Quaternion targetRotation = Quaternion.LookRotation(directionVector);
                float rotationTimer = 0f;
                Quaternion startRotation = transform.rotation;
                while (rotationTimer < 0.1f)
                {
                    rotationTimer += Time.deltaTime;
                    transform.rotation = Quaternion.Slerp(startRotation, targetRotation, rotationTimer / 0.1f);
                    yield return null;
                }
                transform.rotation = targetRotation;
            }

            if (audioSource != null && moveSound != null)
            {
                audioSource.PlayOneShot(moveSound);
                audioSource.PlayOneShot(moveSound);
            }

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

        GameObject currentMasuObject = routeWaypoints[currentWaypointIndex].gameObject;
        MasuEvent masuEvent = currentMasuObject.GetComponentInChildren<MasuEvent>();

        if (currentWaypointIndex == 0 && score >= 3)
        {
            yield return StartCoroutine(GoalPerformanceRoutine());
            yield break;
        }

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

    private IEnumerator GoalPerformanceRoutine()
    {
        isGoal = true;

        if (confettiEffect != null)
        {
            confettiEffect.SetActive(true);
        }

        string dispName = string.IsNullOrEmpty(playerName) ? gameObject.name : playerName;

        if (winTextUI != null)
        {
            winTextUI.text = $"🎉 {dispName} の勝利！ 🎉";
            winTextUI.gameObject.SetActive(true);
        }

        if (logText != null) logText.text = $"🎉 {dispName}がゴールして勝利しました！ 🎉";

        yield return new WaitForSeconds(3.0f);
        SceneManager.LoadScene("ResultScene");
    }

    private void OnLapPassed()
    {
        lapCount++;
        score += 1;
        UpdateUI();
    }

    public bool IsLockingTurn()
    {
        return isProcessingComboEvent;
    }
}