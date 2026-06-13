using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSelectController : MonoBehaviour
{
    // ボタンから呼び出す関数（インスペクターのButtonのOnClickから登録します）
    public void SelectPlayerAndStart(int count)
    {
        if (GameSettings.Instance != null)
        {
            GameSettings.Instance.PlayerCount = count;
        }

        // 本編シーンをロード（本編のシーン名に合わせて変更してください）
        SceneManager.LoadScene("game");
    }
}
