using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // コントローラー操作用
using TMPro; // ★1. これを追加！

public class MemberSelectManager : MonoBehaviour
{
    [Header("UI設定")]
    public TextMeshProUGUI countText; // ★2. 「Text」から「TextMeshProUGUI」に変更！
    public Button firstButton; // 最初にフォーカスを当てるボタン（＋ボタンなど）

    [Header("仮キャラの設定（配列）")]
    public GameObject[] characterObjects; // 6個の仮キャラをここに登録する

    private int currentPlayers = 1; // 最初は1人
    private int maxPlayers = 6; // 最大6人
    private int minPlayers = 1; // 最小1人

    void Start()
    {
        // ゲーム開始時に、ボタンをキーボードやコントローラーで触れる状態にする
        if (firstButton != null)
        {
            firstButton.Select();
        }

        // 最初の表示を更新
        UpdateDisplay();
    }

    // 「＋」ボタンが押されたときに実行する関数
    public void OnPlusButton()
    {
        if (currentPlayers < maxPlayers)
        {
            currentPlayers++;
            UpdateDisplay();
        }
    }

    // 「−」ボタンが押されたときに実行する関数
    public void OnMinusButton()
    {
        if (currentPlayers > minPlayers)
        {
            currentPlayers--;
            UpdateDisplay();
        }
    }

    // 「決定」ボタンが押されたときに実行する関数
    public void OnSubmitButton()
    {
        Debug.Log(currentPlayers + "人でゲームを開始します！(ここに将来遷移の処理を入れます)");
    }

    // 画面の表示をまとめて新しくする関数
    void UpdateDisplay()
    {
        // ① テキストの数字を書き換える
        if (countText != null)
        {
            countText.text = currentPlayers.ToString() + " 人";
        }

        // ② 人数に合わせてキャラの表示・非表示を切り替える
        for (int i = 0; i < characterObjects.Length; i++)
        {
            if (i < currentPlayers)
            {
                characterObjects[i].SetActive(true); // 人数以下なら表示
            }
            else
            {
                characterObjects[i].SetActive(false); // 人数を超えてたら非表示
            }
        }
    }
}