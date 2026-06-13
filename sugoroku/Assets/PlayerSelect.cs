using TMPro; // TextMeshProを使うために必要
using UnityEngine;
using UnityEngine.SceneManagement; // シーン遷移のために必要

public class CharacterSelectUIManager : MonoBehaviour
{
    // --- 【データ引き継ぎ用】ゲーム全体で1つだけのデータ保持の仕組み ---
    public static CharacterSelectUIManager Instance { get; private set; }

    // 別担当の人が「CharacterSelectUIManager.Instance.SelectedCount」で人数を取れるようにする
    public int SelectedCount => playerCount;

    [SerializeField] private TextMeshProUGUI playerCountText; // 人数表示用テキスト
    [SerializeField] private string gameSceneName = "GameScene"; // 別担当が作るゲーム画面のシーン名
    [SerializeField] private GameObject targetUI;


    private int playerCount = 1; // 現在の人数（初期値は1人）
    private int minPlayers = 1; // 最小人数
    private int maxPlayers = 6; // 最大人数

    void Awake()
    {
        // シーンが変わってもこのオブジェクト（と人数データ）が消えないようにする設定
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateUI(); // 起動時に最初のUI表示を行う
    }

    // ＋ボタンが押されたとき
    public void OnPlusButton()
    {
        if (playerCount < maxPlayers)
        {
            playerCount++;
            UpdateUI();
        }
    }

    // －ボタンが押されたとき
    public void OnMinusButton()
    {
        if (playerCount > minPlayers)
        {
            playerCount--;
            UpdateUI();
        }
    }

    // 決定ボタンが押されたとき
    public void OnDecisionButton()
    {
        Debug.Log(playerCount + "人でゲームを開始します！");

        // キャンバス（UI）など、セレクト画面の見た目用オブジェクトを非表示にする
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
            if (targetUI != null)
            {
                targetUI.SetActive(false); // 非表示にする
            }
        }

        // 次のシーン（ゲーム本編）へ遷移
        SceneManager.LoadScene(gameSceneName);
    }

    // ★指示書にあったメソッド
    private void UpdateUI()
    {
        // 画面の文字を書き換える
        playerCountText.text = playerCount.ToString() + "人";

        // ★指示書に書かれていた「連動のための一行」
        // 3Dモデルを生成しているスクリプトを探して、見た目を更新させる
        FindFirstObjectByType<CharacterVisualSpawner>().UpdateCharacterVisuals();
    }
}