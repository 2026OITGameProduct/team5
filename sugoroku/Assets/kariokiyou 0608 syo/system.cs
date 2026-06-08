using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SugorokuPlayer : MonoBehaviour
{
    [SerializeField] private Transform[] routeWaypoints; // マス（Waypoint）を順番に入れた配列
    [SerializeField] private float moveSpeed = 5f;       // 移動速度

    private int currentWaypointIndex = 0; // 現在いるマスの番号
    private bool isMoving = false;

    // 外部（サイコロボタンなど）から呼ばれる関数
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
            // 次のマスに進む（配列の最大数を超えないように注意）
            if (currentWaypointIndex + 1 < routeWaypoints.Length)
            {
                currentWaypointIndex++;
                Vector3 targetPosition = routeWaypoints[currentWaypointIndex].position;

                // 次のマスに到着するまでループ
                while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                    yield return null; // 1フレーム待つ
                }

                transform.position = targetPosition; // 位置を完全に合わせる
                yield return new WaitForSeconds(0.1f); // マスに止まった時のわずかなウエイト
            }
            else
            {
                Debug.Log("ゴールに到達しました！");
                break;
            }
        }

        isMoving = false;
        // ここでマスのイベント（プラス・マイナスなど）を発生させる
    }
}
