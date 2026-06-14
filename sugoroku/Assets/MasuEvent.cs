using UnityEngine;

public class MasuEvent : MonoBehaviour
{
    public enum MasuType
    {
        Normal,         // 普通のマス（何も起きない）
        GoForward3,     // 3マス進む
        GoForward2,     // 2マス進む  <- 🔴 追加
        GoForward1,     // 1マス進む  <- 🔴 追加
        GoBack2,        // 2マス戻る
        GoBack1,        // 1マス戻る  <- 🔴 追加
        AddPoint1,      // 1ポイント得る
        ReducePoint1,   // 1ポイント失う
        MissTurn,       // 1回休み
        StealPoint1     // 他プレイヤーから1pt奪う（0ptなら不発） <- 🔴 追加
    }

    [Header("マスの設定")]
    public MasuType masuType = MasuType.Normal;

    [Header("表示するイベント画像")]
    public Sprite eventImage;

    [Header("🔴 このマス専用の効果音")]
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

            case MasuType.GoForward2: 
                Debug.Log("イベント効果：2マス進みます！");
                player.MoveStepsByEvent(2);
                break;

            case MasuType.GoForward1: 
                Debug.Log("イベント効果：1マス進みます！");
                player.MoveStepsByEvent(1);
                break;

            case MasuType.GoBack2:
                Debug.Log("イベント効果：2マス戻ります！");
                player.MoveBackStepsByEvent(2);
                break;

            case MasuType.GoBack1: 
                Debug.Log("イベント効果：1マス戻ります！");
                player.MoveBackStepsByEvent(1);
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

            case MasuType.StealPoint1: 
                SugorokuManager manager = Object.FindAnyObjectByType<SugorokuManager>();
                if (manager != null)
                {
                    System.Collections.Generic.List<LoopSugorokuPlayer> otherPlayers = new System.Collections.Generic.List<LoopSugorokuPlayer>();
                    
                    foreach (var p in manager.GetAllPlayers())
                    {
                        if (p != player)
                        {
                            otherPlayers.Add(p);
                        }
                    }

                    if (otherPlayers.Count > 0)
                    {
                        int randomIndex = Random.Range(0, otherPlayers.Count);
                        LoopSugorokuPlayer targetPlayer = otherPlayers[randomIndex];

                        // 🔴 相手プレイヤーの表示名を安全に取得する（カスタム名対応）
                        string targetDisplayName = !string.IsNullOrEmpty(targetPlayer.playerName) ? targetPlayer.playerName : targetPlayer.name;

                        if (targetPlayer.score > 0)
                        {
                            targetPlayer.score -= 1;
                            targetPlayer.UpdateUI(); 

                            player.score += 1; 
                            player.UpdateUI(); 
                            Debug.Log($"👥 イベント効果：{targetDisplayName}から1pt奪い取った！");
                        }
                        else
                        {
                            Debug.Log($"👥 イベント効果：{targetDisplayName}を選んだけど0ptだったので奪えなかった！(不発)");
                        }
                    }
                }
                break;
        }
    }
}