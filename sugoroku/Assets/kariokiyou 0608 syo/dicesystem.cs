using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DiceController : MonoBehaviour
{
    [Header("UIコンポーネント")]
    [SerializeField] private Image diceImage;       // 出目を表示するUIのImage
    [SerializeField] private Button rollButton;     // サイコロを振るボタン

    [Header("サイコロ画像（1〜6の順番でセット）")]
    [SerializeField] private Sprite[] diceSprites;  // 要素数6の配列

    [Header("演出設定")]
    [SerializeField] private float rollDuration = 1.0f; // シャッフルする時間（秒）
    [SerializeField] private float shuffleInterval = 0.05f; // 画像が切り替わる速度

    [SerializeField] private LoopSugorokuPlayer player; // (単体プレイ用予備)
    [SerializeField] private SugorokuManager sugorokuManager; // 🔴マネージャー参照

    private bool isRolling = false;

    private void Start()
    {
        if (rollButton != null)
        {
            rollButton.onClick.AddListener(OnRollButtonClick);
        }
    }

    public void OnRollButtonClick()
    {
        if (isRolling) return;

        // 💡【多人数対応】ボタンが押された瞬間に1回休みをチェック
        if (CheckSkipTurn())
        {
            return; // お休みだった場合はサイコロを振らずにパスして終了
        }

        StartCoroutine(RollDiceRoutine());
    }

    private bool CheckSkipTurn()
    {
        // 💡 現在動かすべきターゲットプレイヤーを決定する
        LoopSugorokuPlayer targetPlayer = null;

        if (sugorokuManager != null)
        {
            // マネージャーから「現在の手番のプレイヤー（1Pか2Pか）」をリアルタイムに取得
            targetPlayer = sugorokuManager.GetCurrentPlayer();
        }
        else
        {
            targetPlayer = player;
        }

        // 💡 ターゲットプレイヤーが1回休みフラグを持っていた場合の処理
        if (targetPlayer != null && targetPlayer.isSkippingNextTurn)
        {
            Debug.Log($"🚫 {targetPlayer.name} は1回休みです！パスします。");
            
            // お休みを1回消化したのでフラグを消す
            targetPlayer.isSkippingNextTurn = false;

            if (sugorokuManager != null)
            {
                // マネージャーに出目0（お休み）を伝えて、次の人のターンへ回す
                sugorokuManager.OnDiceRolled(0); 
            }
            else
            {
                if (rollButton != null) rollButton.interactable = true;
            }

            return true; // お休みを適用しました
        }

        return false; // 普通にサイコロを振る
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
    }
}