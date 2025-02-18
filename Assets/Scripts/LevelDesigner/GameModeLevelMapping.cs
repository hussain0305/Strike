using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameModeLevelMapping", menuName = "Game/Game Mode Level Mapping", order = 1)]
public class GameModeLevelMapping : ScriptableObject
{
    private static GameModeLevelMapping instance;
    public static GameModeLevelMapping Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<GameModeLevelMapping>("GameModeLevelMapping");
                if (instance == null)
                {
                    Debug.LogError("GlobalAssets instance not found. Please create one in the Resources folder.");
                }
                else
                {
                    instance.SortLevels();
                }
            }
            return instance;
        }
    }

    public List<GameModeLevelInfo> gameModeLevels;

    private void SortLevels()
    {
        foreach (var info in gameModeLevels)
        {
            info.levels.Sort();
        }
    }
    
    public List<int> GetLevelsForGameMode(GameModeType gameMode)
    {
        foreach (var info in gameModeLevels)
        {
            if (info.gameMode == gameMode)
                return info.levels;
        }
        return new List<int>();
    }

    public int GetNumLevelsInGameMode(GameModeType gameMode)
    {
        foreach (var info in gameModeLevels)
        {
            if (info.gameMode == gameMode)
                return info.levels.Count;
        }
        return 0;
    }
}

[System.Serializable]
public class GameModeLevelInfo
{
    public GameModeType gameMode;
    public List<int> levels;
}