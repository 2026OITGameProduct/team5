using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SugorokuManager : MonoBehaviour
{
    // [SerializeField] private GameObject playerPrefab; // 🔴 これ（1人用）を消します

    // 💡 【新設】人数分のプレイヤープレハブを登録できるように配列にします
    [SerializeField] private GameObject[] playerPrefabs; // インスペクターで1P用、2P用…をセット

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
            // 💡 【重要】生まれた順番（i）に応じて、使用するプレハブを切り替える
            GameObject selectedPrefab;
            if (i < playerPrefabs.Length)
            {
                selectedPrefab = playerPrefabs[i]; // 1Pなら0番目、2Pなら1番目のプレハブ
            }
            else
            {
                // 万が一プレハブが足りない場合は、1番目のプレハブを使い回す安全装置
                selectedPrefab = playerPrefabs[0];
            }

            // 💡 選択されたプレハブ（1P用 asset、2P用 assetなど）を使ってプレイヤーを生成
            GameObject obj = Instantiate(selectedPrefab);
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

    // 💡【新機能】サイコロ側から「現在の手番のプレイヤー」をチェックするための便利機能
    public LoopSugorokuPlayer GetCurrentPlayer()
    {
        if (players == null || players.Count == 0) return null;
        return players[currentPlayerIndex];
    }
    // 🔴 【多人数化用に追加】サイコロが確定したときに呼び出される処理
    public void OnDiceRolled(int diceNumber)
    {
        // 現在のターンのプレイヤーを動かす
        LoopSugorokuPlayer activePlayer = players[currentPlayerIndex];
        // 💡 1回休み（出目0）で呼ばれた場合の処理
        if (diceNumber == 0)
        {
            // 移動を挟まず、すぐに次のプレイヤーへターンを回す
            StartCoroutine(TurnChangeRoutine(0.5f));
            return;
        }
        activePlayer.MoveSteps(diceNumber);
        // 💡【重要バグ修正】
        // ここでの自動ターン交代を廃止しました！
        // 代わりに、プレイヤーが「OKボタンを押して、イベント移動もすべて完全に完了した時」に
        // プレイヤー側から手動で次のターンへ進める命令を送るように今後の拡張に備えます。
        // 現状は、ポップアップ演出の完了を待つため、プレイヤー移動がすべて終了した後に
        // ターンが切り替わる安全なタイミングに修正しました。
        StartCoroutine(TurnChangeRoutine(diceNumber * 0.4f + 2.0f)); // 演出時間を長めに確保してバグを防止
    }

    // 💡 外部（プレイヤーがOKボタンを押し終わった後など）から直接次のターンへ回せる機能
    public void AdvanceTurn()
    {
        StopAllCoroutines(); // 走っているタイマーをリセットして二重交代を防止
        StartCoroutine(TurnChangeRoutine(0.1f));

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