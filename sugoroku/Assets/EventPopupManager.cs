using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EventPopupManager : MonoBehaviour
{
    [SerializeField] private GameObject popupPanel;
    // [SerializeField] private GameObject okButton; // 🔴 古いOKボタンは使わないので不要になります

    private float scaleDuration = 0.5f;  // 四角形が出る速さ：0.5秒
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
        // if (okButton != null) okButton.SetActive(false);
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

        // 🔴 【新設】ポップアップが出終わったら、サイコロボタンを「OKモード」に変身させる
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