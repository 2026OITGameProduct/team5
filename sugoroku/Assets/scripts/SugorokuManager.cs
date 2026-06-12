using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SugorokuManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab; // プレイヤーのプレハブ
    [SerializeField] private Transform[] waypoints;    // 必ず12マスのインスペクターを設定

    // 💡 人数分のUIテキストをインスペクターで登録できるように配列にします
    [SerializeField] private TextMeshProUGUI[] scoreTexts;
    [SerializeField] private TextMeshProUGUI[] lapTexts;
    [SerializeField] private TextMeshProUGUI logText;       // ログは全員で共通の1つ

    // 🔴 【多人数化用に追加】サイコロのスクリプトを紐付けるための変数
    [SerializeField] private DiceController diceController;

    private List<LoopSugorokuPlayer> players = new List<LoopSugorokuPlayer>();

    // 🔴 【多人数化用に追加】現在だれの番か（手番）を記録する変数（0 = 1P, 1 = 2P...）
    private int currentPlayerIndex = 0;

    void Start()
    {
        // ステージセレクトから人数を取得（データがなければ1人）
        int count = PlayerPrefs.GetInt("PlayerCount", 1);

        for (int i = 0; i < count; i++)
        {
            // プレイヤーを生成
            GameObject obj = Instantiate(playerPrefab);
            obj.name = (i + 1) + "P"; // オブジェクトの名前を「1P」「2P」にする

            LoopSugorokuPlayer p = obj.GetComponent<LoopSugorokuPlayer>();

            // 💡 マス、プレイヤー番号、そして対応するUIをプレイヤーに手渡す！
            if (i < scoreTexts.Length && i < lapTexts.Length)
            {
                p.SetupPlayer(waypoints, i, scoreTexts[i], lapTexts[i], logText);
            }
            else
            {
                // UIの箱が足りない場合の安全装置
                p.SetupPlayer(waypoints, i, scoreTexts[0], lapTexts[0], logText);
            }

            players.Add(p);
        }

        // ゲーム開始時の最初のログを表示
        if (logText != null) logText.text = "1Pの番です。サイコロを振ってください！";
    }

    // 🔴 【多人数化用に追加】サイコロが確定したときに呼び出される処理
    public void OnDiceRolled(int diceNumber)
    {
        // 現在のターンのプレイヤーを動かす
        LoopSugorokuPlayer activePlayer = players[currentPlayerIndex];
        activePlayer.MoveSteps(diceNumber);

        // プレイヤーの移動時間を考慮して、少し待ってから次のターンへ進む
        //（出目の数×0.4秒 + 演出用の0.8秒 後にターン交代）
        StartCoroutine(TurnChangeRoutine(diceNumber * 0.4f + 0.8f));
    }

    // 🔴 【多人数化用に追加】ターンを交代させる処理
    private IEnumerator TurnChangeRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 次のプレイヤーの番号にする（人数が2人なら、0→1→0→1 のようにループする計算）
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;

        // ログ画面に次の人の番であることを表示
        if (logText != null)
        {
            logText.text = $"{currentPlayerIndex + 1}Pの番です。サイコロを振ってください！";
        }

        // サイコロのボタンを押せるように戻す
        if (diceController != null)
        {
            diceController.EnableDiceButton();
        }
    }
}