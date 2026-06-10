using UnityEngine;
using TMPro; // TextMeshProを使うために必要

public class PlayerSelect : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerCountText; // 人数表示用テキスト

    private int playerCount = 1; // 現在の人数（初期値は1人）
    private int minPlayers = 1; // 最小人数
    private int maxPlayers = 6; // 最大人数

    void Start()
    {
        UpdateText();
    }

    // ＋ボタンが押されたときの処理
    public void OnPlusButton()
    {
        if (playerCount < maxPlayers)
        {
            playerCount++;
            UpdateText();
        }
    }

    // －ボタンが押されたときの処理
    public void OnMinusButton()
    {
        if (playerCount > minPlayers)
        {
            playerCount--;
            UpdateText();
        }
    }

    // 決定ボタンが押されたときの処理
    public void OnDecisionButton()
    {
        Debug.Log(playerCount + "人でゲームを開始します！");
        // ※ここに次のシーンへ行く処理を書きます
    }

    // 画面の文字を書き換える処理
    private void UpdateText()
    {
        playerCountText.text = playerCount.ToString() + "人";
    }
}
