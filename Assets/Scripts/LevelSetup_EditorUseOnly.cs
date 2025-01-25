using UnityEngine;

public class LevelSetup_EditorUseOnly : MonoBehaviour
{
    private static LevelSetup_EditorUseOnly instance;
    public static LevelSetup_EditorUseOnly Instance => instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public int levelToLoad;
    public GameModeType gameMode;
}
