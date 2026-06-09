using System.Collections;
using UnityEngine;

public class LoopSugorokuPlayer : MonoBehaviour
{
    [SerializeField] private Transform[] routeWaypoints; // 必ず12マスのインスペクターを設定
    [SerializeField] private float moveSpeed = 5f;

    private int currentWaypointIndex = 0;
    private bool isMoving = false;
    private bool isGoal = false;

    // プレイヤーの戦績データ
    public int score { get; private set; } = 0;
    public int lapCount { get; private set; } = 0;

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
            // 12マス（配列の末尾）を超えたら 0 に戻す
            int nextIndex = (currentWaypointIndex + 1) % routeWaypoints.Length;

            // 🔴 インデックスが 0 に戻る＝スタートマス（ゴール）を通過する瞬間
            if (nextIndex == 0)
            {
                // 【重要】通過する「前」の時点で3ポイント持っているかチェック
                if (score >= 3)
                {
                    isGoal = true;
                    currentWaypointIndex = 0;
                    transform.position = routeWaypoints[0].position; // スタートマスにピタッと止める
                    Debug.Log("🎉 3ポイント持った状態でゴールを通過しました！ゲームクリア！ 🎉");
                    isMoving = false;
                    yield break; // 残りの移動をキャンセルして終了
                }

                // 3pt未満なら通常通り周回ボーナスをもらって進む
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
        Debug.Log($"現在 {lapCount} 周目。ボーナス+1pt (総計:{score}pt)");
    }

    // マスに止まったときの処理
    private void OnLandOnWaypoint(int waypointIndex)
    {
        // 3番目と9番目のマスは「プラスマス」
        if (waypointIndex == 3 || waypointIndex == 9)
        {
            score += 1;
            Debug.Log($"プラスマスに到着！ +1pt (総計:{score}pt)");
        }
        // 6番目のマスは「マイナスマス」
        else if (waypointIndex == 6)
        {
            score = Mathf.Max(0, score - 1); // 0未満にならないように引く
            Debug.Log($"マイナスマスに到着... -1pt (総計:{score}pt)");
        }

        // 🔴 もし「3pt持った状態でスタートマスにピッタリ止まった時」もクリアにしたい場合はここを有効に
        if (waypointIndex == 0 && score >= 3)
        {
            isGoal = true;
            Debug.Log("🎉 3ポイント持った状態でスタートにピタッと止まりました！ゲームクリア！ 🎉");
        }
    }
}
