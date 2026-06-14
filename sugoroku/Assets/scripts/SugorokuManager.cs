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

    // 🔴 型名をすべて小文字の「dicesystem」に修正しました
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
            logText.text = $"{playerNames[currentPlayerIndex]}の番です。サイコロを振ってください！";
        }

        if (diceController != null)
        {
            diceController.EnableDiceButton();
        }
    }
}