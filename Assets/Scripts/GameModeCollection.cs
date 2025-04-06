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
        return highestGameModeSceneIndex + 1;
    }
}