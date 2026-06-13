using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EventPopupManager : MonoBehaviour
{
    [SerializeField] private GameObject popupPanel;

    private float scaleDuration = 0.5f;  
    private float buttonDelay = 0.2f;    

    [Header("ポップアップ音の設定")]
    [SerializeField] private AudioSource audioSource;

    private Image panelImage;
    private System.Action onOkPressedCallback;

    // 💡 いま画面の裏で処理を待っているポップアップの数
    private int activePopupCount = 0;

    private void Awake()
    {
        if (popupPanel != null)
        {
            panelImage = popupPanel.GetComponent<Image>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Start()
    {
        if (popupPanel != null) popupPanel.SetActive(false);
    }

    public void ShowEventPopup(Sprite targetSprite, AudioClip eventSound, System.Action onComplete)
    {
        // 💡 ポップアップが要求されるたびに、待ちカウンターを増やす
        activePopupCount++;
        
        onOkPressedCallback = onComplete;

        if (panelImage != null)
        {
            if (targetSprite != null)
                panelImage.sprite = targetSprite;
            else
                panelImage.sprite = null;
        }

        if (audioSource != null && eventSound != null)
        {
            audioSource.PlayOneShot(eventSound);
            audioSource.PlayOneShot(eventSound); 
        }

        StartCoroutine(PopupAnimationRoutine());
    }

    public void ShowEventPopup(Sprite targetSprite, System.Action onComplete)
    {
        ShowEventPopup(targetSprite, null, onComplete);
    }

    private IEnumerator PopupAnimationRoutine()
    {
        popupPanel.transform.localScale = Vector3.zero;
        popupPanel.SetActive(true);

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

        // ポップアップが開いている間は、確実にボタンを「次へ」にする
        dicesystem dice = Object.FindAnyObjectByType<dicesystem>();
        if (dice != null)
        {
            dice.SwitchToOkMode();
        }
    }

    public void OnOkButtonPressed()
    {
        popupPanel.SetActive(false);

        // 💡 1枚閉じたのでカウンターを減らす
        activePopupCount--;

        if (onOkPressedCallback != null)
        {
            // 先にプレイヤー側のイベント（OnPlayerStopなど）を実行する
            onOkPressedCallback.Invoke();
        }

        // 💡【ここが一番重要！】
        // まだ裏にポップアップ（2枚目）が残っているなら、ここでは何もしない（次へを維持）。
        // カウンターが「0」になった＝本当に最後のポップアップが閉じ終わった時だけ、
        // 次のプレイヤーに交代して、ボタンを「サイコロを振る」に戻す処理を走らせる！
        if (activePopupCount <= 0)
        {
            activePopupCount = 0; // 念のためマイナス防止

            // ゲーム全体を管理しているスクリプトを呼び出して、ここで初めてターンを交代させる
            SugorokuManager manager = Object.FindAnyObjectByType<SugorokuManager>();
            if (manager != null)
            {
                // 💡 元々SugorokuManager側でOKボタンを押した時に走っていた、
                // 「ターンを交代してボタンをサイコロに戻す処理」の名前（メソッド名）をここに書いてください。
                // 例：manager.NextTurn(); や manager.OnOkClick(); など
                
                // ※もしメソッド名が分からなければ、SugorokuManager.csのコードを見せていただければすぐに特定します！
            }
        }
    }
}