using System.Collections;
using UnityEngine;
using TMPro; // 💡 TextMeshProを使うために追加

public class LoopSugorokuPlayer : MonoBehaviour
{
    [SerializeField] private Transform[] routeWaypoints; // 必ず12マスのインスペクターを設定
    [SerializeField] private float moveSpeed = 5f;

    // 💡 インスペクターからText（TextMeshPro）を紐付けるための変数
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI lapText;
    [SerializeField] private TextMeshProUGUI logText; // ログ（アナウンス）表示用

    private int currentWaypointIndex = 0;
    private bool isMoving = false;
    private bool isGoal = false;

    // プレイヤーの戦績データ
    public int score { get; private set; } = 0;
    public int lapCount { get; private set; } = 0;

    private void Start()
    {
        // 💡 ゲーム開始時にUIを初期化
        UpdateUI();
        if (logText != null) logText.text = "ゲームスタート！ダイスを振ってください。";
    }

    // 💡 UIの表示を最新の状態に更新するメソッド
    private void UpdateUI()
    {
        if (scoreText != null) scoreText.text = $"ポイント: {score} pt";
        if (lapText != null) lapText.text = $"{lapCount} 周目";
    }

    public void MoveSteps(int steps)
    {
        if (isGoal)
        {
            Debug.Log("すでにゴールしています！");
            return;
        }

        if (!isMoving)
        {
            StartCoroutine(MoveRoutine(steps));
        }
    }

    private IEnumerator MoveRoutine(int steps)
    {
        isMoving = true;

        for (int i = 0; i < steps; i++)
        {
            //12マス(配列の末尾)を超えたら0に戻す
            int nextIndex = (currentWaypointIndex + 1) % routeWaypoints.Length;
            //インデックスが0に戻る=スタートマス(ゴール)を通過する瞬間
            if (nextIndex == 0)
            {
                //[重要]通過する「前」の時点で3ポイント持っているかチェック
                if (score >= 3)
                {
                    isGoal = true;
                    currentWaypointIndex = 0;
                    transform.position = routeWaypoints[0].position;

                    string clearMsg = "🎉 3ポイント持ってゴール通過！ゲームクリア！ 🎉";
                    Debug.Log(clearMsg);
                    if (logText != null) logText.text = clearMsg; // 💡 画面に表示

                    isMoving = false;
                    UpdateUI(); // 💡 ゴール時の状態を反映
                    yield break; //残りの移動をキャンセルして終了
                }
                //3pt未満なら通常通り集会ボーナスをもらって進む
                OnLapPassed();
            }

            currentWaypointIndex = nextIndex;
            Vector3 targetPosition = routeWaypoints[currentWaypointIndex].position;
            // 1マス分の移動処理
            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = targetPosition;
            yield return new WaitForSeconds(0.1f);
        }

        isMoving = false;
        // マスに「着地」したときのイベント判定

        OnLandOnWaypoint(currentWaypointIndex);
    }
    // 周回（通過）したときの処理
    private void OnLapPassed()
    {
        lapCount++;
        score += 1; // 周回ボーナスとして1pt

        string msg = $"周回ボーナス！ +1pt (合計:{score}pt)";
        Debug.Log(msg);
        if (logText != null) logText.text = msg; // 💡 画面に表示

        UpdateUI(); // 💡数値を変更したらUIを更新
    }
    //マスに止まったときの処理
    private void OnLandOnWaypoint(int waypointIndex)
    {
        // 3番目と9番目のマスは「プラスマス」
        if (waypointIndex == 3 || waypointIndex == 9)
        {
            score += 1;
            string msg = $"プラスマスに到着！ +1pt (合計:{score}pt)";
            Debug.Log(msg);
            if (logText != null) logText.text = msg;
        }
        // 6番目のマスは「マイナスマス」
        else if (waypointIndex == 6)
        {
            score = Mathf.Max(0, score - 1);// スコアがマイナスにならないようにする
            string msg = $"マイナスマスに到着... -1pt (合計:{score}pt)";
            Debug.Log(msg);
            if (logText != null) logText.text = msg;
        }
        //もし「3pt持った状態でスタートマスにぴったり止まった時」もクリアにしたい場合はここを有効に
        if (waypointIndex == 0 && score >= 3)
        if (waypointIndex == 0 && score >= 3)
        {
            isGoal = true;
            string msg = "🎉 3ポイント持ってスタートにピッタリ停止！ゲームクリア！ 🎉";
            Debug.Log(msg);
            if (logText != null) logText.text = msg;
        }

        UpdateUI(); // 💡数値を変更したらUIを更新
    }
}
