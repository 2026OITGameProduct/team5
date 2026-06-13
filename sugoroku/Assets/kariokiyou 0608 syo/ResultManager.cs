using UnityEngine;
using TMPro;

public class ResultManager : MonoBehaviour
{
    public TextMeshProUGUI winnerText;

    void Start()
    {
        winnerText.text = "Winner: " + WinnerData.winnerName;
    }
}
