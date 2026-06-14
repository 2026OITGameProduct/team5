using UnityEngine;
using UnityEngine.UI;

public class CharBgColorController : MonoBehaviour
{
    [Header("プレイヤー選択スクリプトをセット")]
    public PlayerSelect playerSelectScript;

    [Header("6つの背景用Imageを左から順にセット")]
    public Image[] bgImages;

    // 各要素（1人目〜6人目）に対応する色の配列
    private Color[] targetColors = new Color[]
    {
        new Color32(139, 69, 19, 255), // 1人目：茶色 (Brown)
        Color.white, // 2人目：白色 (White)
        Color.green, // 3人目：緑色 (Green)
        Color.red, // 4人目：赤色 (Red)
        Color.blue, // 5人目：青色 (Blue)
        Color.yellow // 6人目：黄色 (Yellow)
    };

    // 選択されていない（グレー）の色
    private Color disabledColor = Color.gray;

    void Update()
    {
        if (playerSelectScript == null || bgImages == null) return;

        // 現在選択されている人数を取得（PlayerSelectスクリプト内の変数名に応じて調整が必要な場合があります）
        // ここでは仮に「selectedPlayerCount」という変数（またはプロパティ）があると仮定しています
        int currentPlayers = playerSelectScript.selectedPlayerCount;

        // 6つの背景の色を更新
        for (int i = 0; i < bgImages.Length; i++)
        {
            if (bgImages[i] == null) continue;

            if (i < currentPlayers)
            {
                // 選択されている人数以内なら、それぞれの固有色にする
                if (i < targetColors.Length)
                {
                    bgImages[i].color = targetColors[i];
                }
            }
            else
            {
                // 選ばれていない右側はグレーにする
                bgImages[i].color = disabledColor;
            }
        }
    }
}
