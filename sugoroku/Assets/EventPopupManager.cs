using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EventPopupManager : MonoBehaviour
{
    [SerializeField] private GameObject popupPanel;
    // [SerializeField] private GameObject okButton; // 古いOKボタンは使わないので不要になります

    private float scaleDuration = 0.5f;  // 四角形が出る速さ：0.5秒
    private float buttonDelay = 0.2f;    // OKボタンが出る遅延：0.2秒

    [Header("🔴 ポップアップ音の設定")]
    // 💡 【新設】音を鳴らすためのスピーカー
    [SerializeField] private AudioSource audioSource;

    private Image panelImage;
    private System.Action onOkPressedCallback;

    private void Awake()
    {
        if (popupPanel != null)
        {
            panelImage = popupPanel.GetComponent<Image>();
        }

        // 🔴 【新設】もしスピーカーが未登録なら、自分についているものを自動で取得する
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Start()
    {
        if (popupPanel != null) popupPanel.SetActive(false);
        // if (okButton != null) okButton.SetActive(false);
    }

    // 🔴 【新設】マスの効果音を受け取ってポップアップを開く新しいメソッド
    public void ShowEventPopup(Sprite targetSprite, AudioClip eventSound, System.Action onComplete)
    {
        onOkPressedCallback = onComplete;

        if (panelImage != null)
        {
            if (targetSprite != null)
            {
                panelImage.sprite = targetSprite;
            }
            else
            {
                panelImage.sprite = null;
            }
        }

        // 🔴 【ここがポイント！】ウィンドウが開き始める瞬間に、指定された効果音を重ねて鳴らす！
        if (audioSource != null && eventSound != null)
        {
            audioSource.PlayOneShot(eventSound);
            audioSource.PlayOneShot(eventSound); // 2倍ブースト
        }

        StartCoroutine(PopupAnimationRoutine());
    }

    // 💡 互換性のために古い引数のメソッドも残しておきます（エラー防止）
    public void ShowEventPopup(Sprite targetSprite, System.Action onComplete)
    {
        ShowEventPopup(targetSprite, null, onComplete);
    }

    private IEnumerator PopupAnimationRoutine()
    {
        popupPanel.transform.localScale = Vector3.zero;
        popupPanel.SetActive(true);
        // okButton.SetActive(false); 

        float currentTime = 0f;
        while (currentTime < scaleDuration)
        {
            currentTime += Time.deltaTime;
            float progress = currentTime / scaleDuration;
            popupPanel.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, progress);
            yield return null;
        }
        popupPanel.transform.localScale = Vector3.one;

        yield return new WaitForSeconds(buttonDelay);
        // okButton.SetActive(true);

        // ポップアップが出終わったら、サイコロボタンを「OKモード」に変身させる
        DiceController dice = Object.FindAnyObjectByType<DiceController>();
        if (dice != null)
        {
            dice.SwitchToOkMode();
        }
    }

    public void OnOkButtonPressed()
    {
        popupPanel.SetActive(false);
        // okButton.SetActive(false);

        if (onOkPressedCallback != null)
        {
            onOkPressedCallback.Invoke();
        }
    }
}