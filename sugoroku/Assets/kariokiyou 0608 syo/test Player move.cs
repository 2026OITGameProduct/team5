using System.Collections;
using UnityEngine;

public class LoopSugorokuPlayer : MonoBehaviour
{
    [SerializeField] private Transform[] routeWaypoints; // 必ず12マスのインスペクターを設定
    [SerializeField] private float moveSpeed = 5f;

    private int currentWaypointIndex = 0;
    private bool isMoving = false;

    // プレイヤーの戦績データ
    public int score { get; private set; } = 0;
    public int lapCount { get; private set; } = 0;

    public void MoveSteps(int steps)
    {
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
            // 【重要】12マス（配列の末尾）を超えたら 0 に戻す
            int nextIndex = (currentWaypointIndex + 1) % routeWaypoints.Length;

            // インデックスが 0 に戻る＝スタートマス（周回地点）を通過した瞬間
            if (nextIndex == 0)
            {
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
        score += 50; // 周回ボーナスとして50pt
        Debug.Log($"周回達成！ 現在 {lapCount} 周目。ボーナス+50pt (総計:{score}pt)");
    }

    // マスに止まったときの処理
    private void OnLandOnWaypoint(int waypointIndex)
    {
        // 例: 3番目と9番目のマスは「プラスマス」
        if (waypointIndex == 3 || waypointIndex == 9)
        {
            score += 30;
            Debug.Log($"プラスマスに到着！ +30pt (総計:{score}pt)");
        }
        // 例: 6番目のマスは「マイナスマス」
        else if (waypointIndex == 6)
        {
            score = Mathf.Max(0, score - 20); // 0未満にならないように引く
            Debug.Log($"マイナスマスに到着... -20pt (総計:{score}pt)");
        }
    }
}

