using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("メインカメラ")]
    [SerializeField] private Transform mainCamera;

    [Header("カメラの初期位置（マイナスのY数値）")]
    [SerializeField] private float startY = -7f;

    [Header("目標のY座標（停止位置・通常は0）")]
    [SerializeField] private float targetY = 0f;

    [Header("視点が移動する速度")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("タイトルロゴのCanvasGroup")]
    [SerializeField] private CanvasGroup titleCanvasGroup;

    [Header("スタートボタンのGameObject")]
    [SerializeField] private GameObject startButton;

    [Header("フェードインのスピード")]
    [SerializeField] private float fadeSpeed = 1.5f;

    private bool isMoving = true;
    private CanvasGroup buttonCanvasGroup;

    void Start()
    {
        // カメラを開始位置（下側）に配置
        if (mainCamera != null)
        {
            Vector3 pos = mainCamera.position;
            pos.y = startY;
            mainCamera.position = pos;
        }

        // タイトルを最初は完全に透明（0）にする
        if (titleCanvasGroup != null)
        {
            titleCanvasGroup.alpha = 0f;
        }

        // ボタンのCanvasGroupを取得して透明にする
        if (startButton != null)
        {
            buttonCanvasGroup = startButton.GetComponent<CanvasGroup>();
            if (buttonCanvasGroup != null)
            {
                buttonCanvasGroup.alpha = 0f;
            }
        }
    }

    void Update()
    {
        // 【移動】カメラを下から上へ動かす
        if (isMoving && mainCamera != null)
        {
            Vector3 cameraPos = mainCamera.position;
            if (cameraPos.y < targetY)
            {
                cameraPos.y += moveSpeed * Time.deltaTime;

                if (cameraPos.y >= targetY)
                {
                    cameraPos.y = targetY;
                    isMoving = false; // カメラ停止
                }

                mainCamera.position = cameraPos;
            }
        }

        // 【フェード】カメラが完全に「止まってから」浮き出させる
        if (!isMoving)
        {
            // 1. タイトルロゴをじわ〜っと表示
            if (titleCanvasGroup != null && titleCanvasGroup.alpha < 1f)
            {
                titleCanvasGroup.alpha += fadeSpeed * Time.deltaTime;
            }

            // 2. タイトルロゴが半分以上見えたら、スタートボタンもじわ〜っと表示
            if (titleCanvasGroup != null && titleCanvasGroup.alpha > 0.5f)
            {
                if (buttonCanvasGroup != null && buttonCanvasGroup.alpha < 1f)
                {
                    buttonCanvasGroup.alpha += fadeSpeed * Time.deltaTime;
                }
            }
        }
    }
}