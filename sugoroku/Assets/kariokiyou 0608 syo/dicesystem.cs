using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class dicesystem : MonoBehaviour
{
    [Header("UIコンポーネント")]
    [SerializeField] private Image diceImage;       
    [SerializeField] private Button rollButton;     
    [SerializeField] private TextMeshProUGUI buttonText; 

    [Header("サイコロ画像（1〜6の順番でセット）")]
    [SerializeField] private Sprite[] diceSprites;  

    [Header("演出設定")]
    [SerializeField] private float rollDuration = 1.0f; 
    [SerializeField] private float shuffleInterval = 0.05f; 

    [Header("オーディオ設定")]
    [SerializeField] private AudioClip rollSound;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private LoopSugorokuPlayer player;
    [SerializeField] private SugorokuManager sugorokuManager;

    private bool isRolling = false;
    private bool isOkMode = false; 

    private void Start()
    {
        if (rollButton != null)
        {
            rollButton.onClick.AddListener(OnRollButtonClick);
        }
        UpdateVectorButtonText("サイコロを振る"); 

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void OnRollButtonClick()
    {
        if (isOkMode)
        {
            EventPopupManager popup = Object.FindAnyObjectByType<EventPopupManager>();
            if (popup != null)
            {
                // 現在のプレイヤーを取得する
                LoopSugorokuPlayer currentPlayer = null;
                if (sugorokuManager != null)
                {
                    currentPlayer = sugorokuManager.GetCurrentPlayer();
                }
                else
                {
                    currentPlayer = player;
                }

                // 🛠️ 修正：名前を IsLockingTurn() に統一しました！
                // 連続イベントの真っ最中の場合は、次へモードを維持したままポップアップだけを閉じる
                if (currentPlayer != null && currentPlayer.IsLockingTurn())
                {
                    popup.OnOkButtonPressed(); 
                }
                else
                {
                    // 連続イベントが絡まない、あるいは本当に最後の終了時
                    isOkMode = false; 
                    popup.OnOkButtonPressed(); 
                }
            }
            return;
        }

        if (isRolling) return;

        if (CheckSkipTurn())
        {
            return; 
        }

        if (audioSource != null && rollSound != null)
        {
            audioSource.PlayOneShot(rollSound);
        }

        StartCoroutine(RollDiceRoutine());
    }

    private bool CheckSkipTurn()
    {
        LoopSugorokuPlayer targetPlayer = null;

        if (sugorokuManager != null)
        {
            targetPlayer = sugorokuManager.GetCurrentPlayer();
        }
        else
        {
            targetPlayer = player;
        }

        if (targetPlayer != null && targetPlayer.isSkippingNextTurn)
        {
            Debug.Log($"🚫 {targetPlayer.name} は1回休みです！パスします。");
            targetPlayer.isSkippingNextTurn = false;

            if (sugorokuManager != null)
            {
                sugorokuManager.OnDiceRolled(0); 
            }
            else
            {
                if (rollButton != null) rollButton.interactable = true;
            }

            return true; 
        }

        return false; 
    }

    private IEnumerator RollDiceRoutine()
    {
        isRolling = true;
        rollButton.interactable = false; 

        float timer = 0f;
        int lastRandomIndex = -1;
        int finalResult = 1;

        while (timer < rollDuration)
        {
            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, 6); 
            } while (randomIndex == lastRandomIndex);

            lastRandomIndex = randomIndex;
            diceImage.sprite = diceSprites[randomIndex]; 

            timer += shuffleInterval;
            yield return new WaitForSeconds(shuffleInterval);
        }

        finalResult = Random.Range(1, 7); 
        diceImage.sprite = diceSprites[finalResult - 1];

        Debug.Log($"サイコロの出目: {finalResult}");

        yield return new WaitForSeconds(0.5f); 

        if (sugorokuManager != null)
        {
            sugorokuManager.OnDiceRolled(finalResult);
        }
        else if (player != null)
        {
            player.MoveSteps(finalResult); 
            rollButton.interactable = true; 
        }

        isRolling = false;
    }

    public void EnableDiceButton()
    {
        if (rollButton != null) rollButton.interactable = true;
        UpdateVectorButtonText("サイコロを振る"); 
    }

    public void SwitchToOkMode()
    {
        isOkMode = true;
        if (rollButton != null) rollButton.interactable = true; 
        UpdateVectorButtonText("次へ"); 
    }

    private void UpdateVectorButtonText(string text)
    {
        if (buttonText != null) buttonText.text = text;
    }
}