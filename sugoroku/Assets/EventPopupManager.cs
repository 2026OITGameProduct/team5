using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EventPopupManager : MonoBehaviour
{
    [SerializeField] private GameObject popupPanel; 
    [SerializeField] private GameObject okButton; 
    
    // 💡 タイマーを速く調整しました
    private float scaleDuration = 0.3f;  // 四角形が出る速さ：0.3秒
    private float buttonDelay = 0.2f;    // OKボタンが出る遅延：0.2秒

    private Image panelImage; 
    private System.Action onOkPressedCallback; 

    private void Awake()
    {
        if (popupPanel != null)
        {
            panelImage = popupPanel.GetComponent<Image>();
        }
    }

    private void Start()
    {
        if (popupPanel != null) popupPanel.SetActive(false);
        if (okButton != null) okButton.SetActive(false);
    }

    public void ShowEventPopup(Sprite targetSprite, System.Action onComplete)
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

        StartCoroutine(PopupAnimationRoutine());
    }

    private IEnumerator PopupAnimationRoutine()
    {
        // 1. 小さい状態で出現
        popupPanel.transform.localScale = Vector3.zero;
        popupPanel.SetActive(true);
        okButton.SetActive(false); 

        // 2. 0.5秒で大きくなる
        float currentTime = 0f;
        while (currentTime < scaleDuration)
        {
            currentTime += Time.deltaTime;
            float progress = currentTime / scaleDuration;
            popupPanel.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, progress);
            yield return null;
        }
        popupPanel.transform.localScale = Vector3.one; 

        // 3. 大きくなった0.2秒後に
        yield return new WaitForSeconds(buttonDelay);

        // 4. OKボタン出現
        okButton.SetActive(true);
    }

    public void OnOkButtonPressed()
    {
        popupPanel.SetActive(false);
        okButton.SetActive(false);

        if (onOkPressedCallback != null)
        {
            onOkPressedCallback.Invoke();
        }
    }
}