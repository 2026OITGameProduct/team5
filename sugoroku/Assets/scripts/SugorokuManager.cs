using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SugorokuManager : MonoBehaviour
{
    [SerializeField] private GameObject[] playerPrefabs;
    [SerializeField] private Transform[] waypoints;

    [SerializeField] private TextMeshProUGUI[] scoreTexts;
    [SerializeField] private TextMeshProUGUI[] lapTexts;
    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField] private string[] playerNames = new string[6];

    [SerializeField] private dicesystem diceController;

    private List<LoopSugorokuPlayer> players = new List<LoopSugorokuPlayer>();
    private int currentPlayerIndex = 0;

    void Start()
    {
        int count = PlayerPrefs.GetInt("PlayerCount", 1);

        for (int i = 0; i < count; i++)
        {
            GameObject selectedPrefab;
            if (i < playerPrefabs.Length)
            {
                selectedPrefab = playerPrefabs[i];
            }
            else
            {
                selectedPrefab = playerPrefabs[0];
            }

            GameObject obj = Instantiate(selectedPrefab);
            obj.name = (i + 1) + "P";

            LoopSugorokuPlayer p = obj.GetComponent<LoopSugorokuPlayer>();

            // インスペクターの「Player Names」に書かれた名前を安全に取得する
            string currentName = (i < playerNames.Length && !string.IsNullOrEmpty(playerNames[i]))
                ? playerNames[i]
                : (i + 1) + "P"; // もし空欄なら「1P」「2P」にする防衛策

            if (i < scoreTexts.Length && i < lapTexts.Length)
            {
                // 最後の引数に「currentName」を追加してプレイヤーに名前を渡す！
                p.SetupPlayer(waypoints, i, scoreTexts[i], lapTexts[i], logText, currentName);
            }
            else
            {
                p.SetupPlayer(waypoints, i, scoreTexts[0], lapTexts[0], logText, currentName);
            }

            players.Add(p);
        }

        // ログテキストも「1Pの番です」から「最初のプレイヤー名」に連動するようにスマート化！
        string firstPlayerName = (playerNames.Length > 0 && !string.IsNullOrEmpty(playerNames[0])) ? playerNames[0] : "1P";
        if (logText != null) logText.text = $"{firstPlayerName}の番です。サイコロを振ってください！";
    }

    public LoopSugorokuPlayer GetCurrentPlayer()
    {
        if (players == null || players.Count == 0) return null;
        return players[currentPlayerIndex];
    }

    public void OnDiceRolled(int diceNumber)
    {
        LoopSugorokuPlayer activePlayer = players[currentPlayerIndex];

        if (diceNumber == 0)
        {
            StartCoroutine(TurnChangeRoutine(0.5f));
            return;
        }

        activePlayer.MoveSteps(diceNumber);
        StartCoroutine(TurnChangeRoutine(diceNumber * 0.4f + 2.0f));
    }

    public void AdvanceTurn()
    {
        StopAllCoroutines();
        StartCoroutine(TurnChangeRoutine(0.1f));
    }

    private IEnumerator TurnChangeRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;

        if (logText != null)
        {
            // 念のためここも安全に名前を引っ張る形に統一
            string nextPlayerName = (currentPlayerIndex < playerNames.Length && !string.IsNullOrEmpty(playerNames[currentPlayerIndex]))
                ? playerNames[currentPlayerIndex]
                : players[currentPlayerIndex].gameObject.name;

            logText.text = $"{nextPlayerName}の番です。サイコロを振ってください！";
        }

        if (diceController != null)
        {
            diceController.EnableDiceButton();
        }
    }

    // 🛠️ 最終追加：MasuEvent.cs から全プレイヤーのリストを参照させて「ポイント強奪効果」を動かすための窓口
    public List<LoopSugorokuPlayer> GetAllPlayers()
    {
        return players;
    }
}