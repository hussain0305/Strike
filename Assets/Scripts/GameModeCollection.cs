using UnityEngine;

[CreateAssetMenu(fileName = "GameModeCollection", menuName = "Game/Game Mode Collection", order = 2)]
public class GameModeCollection : ScriptableObject
{
    public GameModeInfo[] gameModes;

    public GameModeInfo GetGameModeInfo(GameModeType type)
    {
        foreach (GameModeInfo modeInfo in gameModes)
        {
            if (modeInfo.gameMode == type)
            {
                return modeInfo;
            }
        }

        return gameModes[0];
    }

    public int GetEndlessLevel()
    {
        int highestGameModeSceneIndex = 0;
        foreach (GameModeInfo modeInfo in gameModes)
        {
            if (modeInfo.scene > highestGameModeSceneIndex)
            {
                highestGameModeSceneIndex = modeInfo.scene;
            }
        }
        return highestGameModeSceneIndex + 1;
    }

    public int GetTutorialLevel()
    {
        int highestGameModeSceneIndex = 0;
        foreach (GameModeInfo modeInfo in gameModes)
        {
            if (modeInfo.scene > highestGameModeSceneIndex)
            {
                highestGameModeSceneIndex = modeInfo.scene;
            }
        }
        return highestGameModeSceneIndex + 2;
    }

    public GameModeType GetNextGameMode(GameModeType gameMode)
    {
        for (int i = 0; i < gameModes.Length; i++)
        {
            if (gameModes[i].gameMode == gameMode)
            {
                return gameModes[(i + 1) % gameModes.Length].gameMode;
            }
        }
        return gameModes[0].gameMode;
    }
    
    public GameModeType GetPreviousGameMode(GameModeType gameMode)
    {
        for (int i = 0; i < gameModes.Length; i++)
        {
            if (gameModes[i].gameMode == gameMode)
            {
                return gameModes[(i - 1 + gameModes.Length) % gameModes.Length].gameMode;
            }
        }
        return gameModes[0].gameMode;
    }
}