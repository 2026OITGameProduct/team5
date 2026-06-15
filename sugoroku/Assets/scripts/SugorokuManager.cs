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

            string currentName = (i < playerNames.Length && !string.IsNullOrEmpty(playerNames[i]))
                ? playerNames[i]
                : (i + 1) + "P";

            if (i < scoreTexts.Length && i < lapTexts.Length)
            {
                p.SetupPlayer(waypoints, i, scoreTexts[i], lapTexts[i], logText, currentName);
            }
            else
            {
                p.SetupPlayer(waypoints, i, scoreTexts[0], lapTexts[0], logText, currentName);
            }

            players.Add(p);
        }

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

        // イベント終了の通知(0)が来たら、ディレイを挟まず即座に手番を切り替える
        if (diceNumber == 0)
        {
            ChangeToNextTurnImmediate();
            return;
        }

        activePlayer.MoveSteps(diceNumber);
    }

    public void AdvanceTurn()
    {
        ChangeToNextTurnImmediate();
    }

    private void ChangeToNextTurnImmediate()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;

        if (logText != null)
        {
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

    // 🛠️【復活！】 MasuEvent.cs から全プレイヤーのリストを参照させて「ポイント強奪」などを動かすための窓口
    public List<LoopSugorokuPlayer> GetAllPlayers()
    {
        return players;
    }
}