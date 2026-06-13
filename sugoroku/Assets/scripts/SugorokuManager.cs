using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SugorokuManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab; // プレイヤーのプレハブ
    [SerializeField] private Transform[] waypoints;    // 必ず12マスのインスペクターを設定

    [SerializeField] private TextMeshProUGUI[] scoreTexts;
    [SerializeField] private TextMeshProUGUI[] lapTexts;
    [SerializeField] private TextMeshProUGUI logText;       // ログは全員で共通の1つ

    [SerializeField] private DiceController diceController;

    private List<LoopSugorokuPlayer> players = new List<LoopSugorokuPlayer>();
    private int currentPlayerIndex = 0;

    void Start()
    {
        int count = PlayerPrefs.GetInt("PlayerCount", 1);

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(playerPrefab);
            obj.name = (i + 1) + "P";

            LoopSugorokuPlayer p = obj.GetComponent<LoopSugorokuPlayer>();

            if (i < scoreTexts.Length && i < lapTexts.Length)
            {
                p.SetupPlayer(waypoints, i, scoreTexts[i], lapTexts[i], logText);
            }
            else
            {
                p.SetupPlayer(waypoints, i, scoreTexts[0], lapTexts[0], logText);
            }

            players.Add(p);
        }

        if (logText != null) logText.text = "1Pの番です。サイコロを振ってください！";
    }

    // 💡【新機能】サイコロ側から「現在の手番のプレイヤー」をチェックするための便利機能
    public LoopSugorokuPlayer GetCurrentPlayer()
    {
        if (players == null || players.Count == 0) return null;
        return players[currentPlayerIndex];
    }

    public void OnDiceRolled(int diceNumber)
    {
        LoopSugorokuPlayer activePlayer = players[currentPlayerIndex];

        // 💡 1回休み（出目0）で呼ばれた場合の処理
        if (diceNumber == 0)
        {
            // 移動を挟まず、すぐに次のプレイヤーへターンを回す
            StartCoroutine(TurnChangeRoutine(0.5f));
            return;
        }

        // 通常のサイコロ移動を開始
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

    private IEnumerator TurnChangeRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;

        if (logText != null)
        {
            logText.text = $"{currentPlayerIndex + 1}Pの番です。サイコロを振ってください！";
        }

        if (diceController != null)
        {
            diceController.EnableDiceButton();
        }
    }
}