using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameModeLevelMapping", menuName = "Game/Game Mode Level Mapping", order = 1)]
public class GameModeLevelMapping : ScriptableObject
{
    public List<GameModeLevelInfo> gameModeLevels;

    public List<int> GetLevelsForGameMode(GameModeType gameMode)
    {
        foreach (var info in gameModeLevels)
        {
            if (info.gameMode == gameMode)
                return info.levels;
        }
        return new List<int>();
    }
}

[System.Serializable]
public class GameModeLevelInfo
{
    public GameModeType gameMode;
    public List<int> levels;
}