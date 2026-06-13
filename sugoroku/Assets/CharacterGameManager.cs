using UnityEngine;

public class CharacterGameManager : MonoBehaviour
{
    // 別のスクリプトから「CharacterGameManager.Instance.SelectedCount」で人数を取得できるようにする
    public static CharacterGameManager Instance { get; private set; }

    [Header("Player Count Settings")]
    [SerializeField] private int minPlayers = 1;
    [SerializeField] private int maxPlayers = 4;

    // 現在選択されている人数（外部からは読み取り専用）
    public int SelectedCount { get; private set; }

    private void Awake()
    {
        // シングルトンの設定
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 初期値を最小人数に合わせる
            SelectedCount = minPlayers;
        }
        else
        {
            Debug.LogWarning($"[CharacterGameManager] 重複したインスタンスを破棄しました: {gameObject.name}");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 人数を増やすメソッド
    /// </summary>
    public void IncreaseCount()
    {
        if (SelectedCount < maxPlayers)
        {
            SelectedCount++;
        }
    }

    /// <summary>
    /// 人数を減らすメソッド
    /// </summary>
    public void DecreaseCount()
    {
        if (SelectedCount > minPlayers)
        {
            SelectedCount--;
        }
    }
}