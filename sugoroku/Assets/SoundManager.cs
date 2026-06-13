#nullable enable
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // Unity 6の標準的なNull許容表記（?を外しても機能しますが一応維持します）
    public static SoundManager? Instance { get; private set; }

    // インスペクターで設定する、そのシーン固有のBGMやSEのソース
    [Header("シーン切り替え時に本物に引き継ぐオーディオソース")]
    [SerializeField] private AudioSource? sceneAudioSource;

    void Awake()
    {
        // すでに本物がいる場合（シーン切り替えで2個目が生まれた時）
        if (Instance != null && Instance != this)
        {
            // 【重要】新シーンの音データを、生き残る本物に引っ越しさせる
            if (sceneAudioSource != null && sceneAudioSource.clip != null)
            {
                // 本物のAudioSourceに、このシーンで鳴らしたかった音をセットして再生
                Instance.PlaySceneAudio(sceneAudioSource.clip);
            }

            // 引き継ぎが終わったので、自分（偽物）は消滅
            Destroy(gameObject);
            return;
        }

        // 最初に見つかったSoundManagerを本物として登録
        Instance = this;

        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    // 本物が新シーンの音を受け取って鳴らすためのメソッド
    public void PlaySceneAudio(AudioClip clip)
    {
        // 本物自身が持っているAudioSourceを取得して再生する
        AudioSource mySource = GetComponent<AudioSource>();
        if (mySource != null)
        {
            mySource.clip = clip;
            mySource.Play();
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}