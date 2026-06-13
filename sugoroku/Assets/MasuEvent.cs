using UnityEngine;

public class MasuEvent : MonoBehaviour
{
    public enum MasuType
    {
        Normal,         // 普通のマス（何も起きない）
        GoForward3,     // 3マス進む
        GoBack2,        // 2マス戻る
        AddPoint1,      // 1ポイント得る
        ReducePoint1,   // 1ポイント失う
        MissTurn        // 1回休み
    }

    [Header("マスの設定")]
    public MasuType masuType = MasuType.Normal;

    [Header("表示するイベント画像")]
    public Sprite eventImage;

    [Header("🔴 このマス専用の効果音")]
    // 💡 【新設】このマスに止まってウィンドウが開くときに鳴らしたい音をインスペクターで入れます
    public AudioClip masuEventSound;

    // OKボタンが押された後に実行される中身
    public void OnPlayerStop(LoopSugorokuPlayer player)
    {
        switch (masuType)
        {
            case MasuType.Normal:
                break;

            case MasuType.GoForward3:
                Debug.Log("イベント効果：3マス進みます！");
                player.MoveStepsByEvent(3);
                break;

            case MasuType.GoBack2:
                Debug.Log("イベント効果：2マス戻ります！");
                player.MoveBackStepsByEvent(2);
                break;

            case MasuType.AddPoint1:
                player.score += 1;
                player.UpdateUI();
                Debug.Log($"イベント効果：1ポイント獲得！ (合計: {player.score}pt)");
                break;

            case MasuType.ReducePoint1:
                player.score = Mathf.Max(0, player.score - 1);
                player.UpdateUI();
                Debug.Log($"イベント効果：1ポイント減少... (合計: {player.score}pt)");
                break;

            case MasuType.MissTurn:
                Debug.Log("イベント効果：次のターンは1回休みになります！");
                player.isSkippingNextTurn = true;
                break;
        }
    }
}