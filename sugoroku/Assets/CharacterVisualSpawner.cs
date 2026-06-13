using UnityEngine;
using System.Collections.Generic;

public class CharacterVisualSpawner : MonoBehaviour
{
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private Transform spawnParent;
    [SerializeField] private float spaceBetween = 2.0f;

    private List<GameObject> spawnedCharacters = new List<GameObject>();

    private void Start()
    {
        // ここはタイミングのズレを防ぐため、一旦空っぽにしています
    }

    public void UpdateCharacterVisuals()
    {
        Debug.Log("【デバッグ1】UpdateCharacterVisualsが呼ばれました。");

        if (characterPrefab == null)
        {
            Debug.LogError("【エラー】characterPrefab がインスペクターで設定されていません！");
            return;
        }
        if (spawnParent == null)
        {
            Debug.LogError("【エラー】spawnParent がインスペクターで設定されていません！");
            return;
        }

        if (CharacterSelectUIManager.Instance == null)
        {
            Debug.LogWarning("【警告】CharacterSelectUIManagerが見つかりません。");
            return;
        }

        // 2. 今表示されているキャラを一度全員削除
        Debug.Log($"【デバッグ2】古いキャラを削除します。現在のリスト数: {spawnedCharacters.Count}");
        foreach (var character in spawnedCharacters)
        {
            if (character != null) Destroy(character);
        }
        spawnedCharacters.Clear();

        // 3. 現在の人数を取得
        int currentCount = CharacterSelectUIManager.Instance.SelectedCount;
        Debug.Log($"【デバッグ3】取得した選択人数は: {currentCount} 人です。");

        // 4. その人数分新たに生成
        for (int i = 0; i < currentCount; i++)
        {
            float offsetX = (i - (currentCount - 1) / 2.0f) * spaceBetween;
            Vector3 spawnPosition = spawnParent.position + new Vector3(offsetX, 0, 0);

            Debug.Log($"【デバッグ4】キャラを生成します。位置: {spawnPosition}");

            GameObject newChar = Instantiate(characterPrefab, spawnPosition, spawnParent.rotation, spawnParent);
            spawnedCharacters.Add(newChar);
        }

        Debug.Log($"【デバッグ5】生成処理が終了しました。合計生成数: {spawnedCharacters.Count}");
    }

    private void OnDestroy()
    {
        foreach (var character in spawnedCharacters)
        {
            if (character != null) Destroy(character);
        }
        spawnedCharacters.Clear();
    }
}
