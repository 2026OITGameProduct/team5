using UnityEngine;

public class BGMManager : MonoBehaviour
{
    private static BGMManager instance;

    void Awake()
    {
        // 最初に作られたBGMManagerを記録して、シーンが切り替わっても壊さないようにする
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        // もし次のシーンに別のBGMManagerが置いてあったら、重複して音が二重に鳴らないように自分を消す
        else
        {
            Destroy(gameObject);
        }
    }
}