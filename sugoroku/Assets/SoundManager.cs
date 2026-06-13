#nullable enable
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // 世界に1つだけのSoundManagerを記憶する箱（外部からもアクセス可能）
    // 「?」をつけることで、Unity 6のNull許容参照型（警告メッセージ）に対応
    public static SoundManager? Instance { get; private set; }

    void Awake()
    {
        // すでに本物がいるのに新しく作られたら、偽物なので即座に消滅させる
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return; // 破棄した後は後ろの処理を行わない
        }

        // 最初に見つかったSoundManagerを本物として登録
        Instance = this;

        // シーンが変わっても消えないようにする（ルートオブジェクトの場合のみ）
        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning($"{name} は親オブジェクトの下にあるため、DontDestroyOnLoad が効きません。ヒエラルキーの最上級（ルート）に配置してください。");
        }
    }

    void OnDestroy()
    {
        // 自分自身（本物）が破棄されるタイミングで、記憶箱を空っぽにする
        // これにより Unity 6 の高速起動モード（Domain Reload Off）でもバグらなくなります
        if (Instance == this)
        {
            Instance = null;
        }
    }
}