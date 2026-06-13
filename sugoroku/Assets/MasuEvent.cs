using UnityEngine;

public class MasuEvent : MonoBehaviour
{
    // 将来的に「3マス進む」などの効果を選びたくなったとき用の枠
    public enum MasuType { Normal, GoForward3, GoBack2, MissTurn }
    public MasuType masuType = MasuType.Normal;

    // 💡【パネルを後で画像に差し替えられる枠！】
    // インスペクターから、このマス専用の画像をここに入れます。空っぽなら白いパネルが出ます。
    public Sprite eventImage;  

    // 💡【後々イベントを実行できる枠！】
    // OKボタンが押された後に実行したい中身をここに書きます。今はまだ「消えるだけ」にしたいので、何も起きないようにしています。
    public void OnPlayerStop(LoopSugorokuPlayer player)
    {
        switch (masuType)
        {
            case MasuType.Normal:
                // 今は何もしない（四角形が消えて終わり）
                break;
                
            case MasuType.GoForward3:
                player.MoveStepsByEvent(3); 
                break;
        }
    }
}