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

    // プレイヤーへの参照（出目を渡すため）
    [SerializeField] private LoopSugorokuPlayer player;

    // 🔴 【多人数化用に追加】マネージャーへの参照
    [SerializeField] private SugorokuManager sugorokuManager;

    private bool isRolling = false;

    private void Start()
    {
        // ボタンにクリックイベントを登録
        if (rollButton != null)
        {
            rollButton.onClick.AddListener(OnRollButtonClick);
        }
    }

    // ボタンが押された時の処理
    public void OnRollButtonClick()
    {
        if (isRolling) return;
        StartCoroutine(RollDiceRoutine());
    }

    private IEnumerator RollDiceRoutine()
    {
        isRolling = true;
        rollButton.interactable = false; // 連打防止でボタンを無効化

        float timer = 0f;
        int lastRandomIndex = -1;
        int finalResult = 1;

        // 指定した時間（rollDuration）の間、画像を高速でシャッフルする
        while (timer < rollDuration)
        {
            int randomIndex;
            // 前のフレームと同じ画像が連続で選ばれないようにする工夫
            do
            {
                randomIndex = Random.Range(0, 6); // 0〜5のインデックス
            } while (randomIndex == lastRandomIndex);

            lastRandomIndex = randomIndex;
            diceImage.sprite = diceSprites[randomIndex]; // 画像を変更

            timer += shuffleInterval;
            yield return new WaitForSeconds(shuffleInterval);
        }

        // 【最終決定】1〜6のランダムな出目を決定
        finalResult = Random.Range(1, 7); // 1〜6の整数

        // 確定した出目の画像をセット（配列は0から始まるので -1 する）
        diceImage.sprite = diceSprites[finalResult - 1];

        Debug.Log($"サイコロの出目: {finalResult}");

        yield return new WaitForSeconds(0.5f); // 確定目を少し見せるためのウェイト

        // プレイヤーを移動させる（前回のスクリプトを呼び出す）
        if (sugorokuManager != null)
        {
            // 🔴 多人数時はマネージャーに出目を渡して現在のプレイヤーを動かす
            sugorokuManager.OnDiceRolled(finalResult);
        }
        else if (player != null)
        {
            // （予備用）もしマネージャーがいなければ、元の単体プレイヤーを動かす
            player.MoveSteps(finalResult); // 確定した出目の数を渡す
            rollButton.interactable = true; // ボタンを再度有効化
        }

        isRolling = false;
    }

    // 🔴 【多人数化用に追加】プレイヤーの移動終了後にマネージャーからボタンを復活させる
    public void EnableDiceButton()
    {
        if (rollButton != null) rollButton.interactable = true;
    }
}