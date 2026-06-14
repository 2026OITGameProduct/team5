using UnityEngine;
using System.Collections.Generic;

public class IconManager : MonoBehaviour
{
    [Header("色違いのアイコン6個を順番にドラッグ＆ドロップ")]
    [SerializeField] private List<GameObject> playerIcons;

    /// <summary>
    /// 指定された人数に合わせてアイコンの表示・非表示を切り替える
    /// </summary>
    /// <param name="count">現在のプレイヤー人数</param>
    public void UpdateIcons(int count)
    {
        for (int i = 0; i < playerIcons.Count; i++)
        {
            // 人数より小さいインデックスのアイコンを表示、それ以外を非表示
            if (i < count)
            {
                playerIcons[i].SetActive(true);
            }
            else
            {
                playerIcons[i].SetActive(false);
            }
        }
    }
}