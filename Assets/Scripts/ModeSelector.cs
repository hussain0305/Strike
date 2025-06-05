using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameModeChangedEvent
{
    public GameModeInfo GameModeInfo;
    public GameModeChangedEvent(GameModeInfo gameModeInfo)
    {
        GameModeInfo = gameModeInfo;
    }
}
public class NumPlayersChangedEvent
{
    public int numPlayers;
    public NumPlayersChangedEvent(int _num)
    {
        numPlayers = _num;
    }
}

public class ModeSelector : MonoBehaviour
{
    [Header("Game Mode")]
    public GameModeCollection gameModeInfo;
    
    private int maxPlayers = 8;
    public int MaxPlayers => maxPlayers;
    
    private int currentNumPlayers = 1;
    private int selectedLevel = 1;
    
    public bool IsPlayingSolo => currentNumPlayers == 1;

    private GameModeInfo currentSelectedModeInfo;
    public GameModeInfo CurrentSelectedModeInfo => currentSelectedModeInfo;

    private GameModeType currentSelectedMode;
    public GameModeType CurrentSelectedMode => currentSelectedMode;
    
    private GameModeInfo endlessGameMode;
    
    private void OnEnable()
    {
        EventBus.Subscribe<ButtonClickedEvent>(SomeButtonClicked);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ButtonClickedEvent>(SomeButtonClicked);
    }

    public void Init()
    {
        endlessGameMode = new GameModeInfo();
        endlessGameMode.gameMode = GameModeType.Endless;
        endlessGameMode.scene = GetEndlessLevel();
        endlessGameMode.displayName = "Endless";
        
        GameModeSelected((GameModeType)SaveManager.GetLastPlayedGauntletMode());
    }

    public void AddPlayer()
    {
        if (currentNumPlayers >= maxPlayers)
        {
            return;
        }

        currentNumPlayers++;
        NumPlayersChanged();
    }

    public void RemovePlayer()
    {
        if (currentNumPlayers <= 1)
        {
            return;
        }
        
        currentNumPlayers--;
        
        NumPlayersChanged();
    }

    public void NumPlayersChanged()
    {
        currentNumPlayers = Mathf.Clamp(currentNumPlayers, 1, maxPlayers);
        EventBus.Publish(new NumPlayersChangedEvent(currentNumPlayers));
    }

    public void NextGameMode()
    {
        currentSelectedMode = (GameModeType)(((int)currentSelectedMode + 1) % gameModeInfo.gameModes.Length);
        Debug.Log("Next Game Mode: " + currentSelectedMode);
        GameModeSelected(currentSelectedMode);
    }

    public void PreviousGameMode()
    {
        int totalModes = gameModeInfo.gameModes.Length;
        currentSelectedMode = (GameModeType)(((int)currentSelectedMode - 1 + totalModes) % totalModes);
        Debug.Log("Previous Game Mode: " + currentSelectedMode);
        GameModeSelected(currentSelectedMode);
    }

    public void GameModeSelected(GameModeType currentSelected)
    {
        ResetSelectedLevel();
        currentSelectedMode = currentSelected;
        currentSelectedModeInfo = gameModeInfo.GetGameModeInfo(currentSelected);
        EventBus.Publish(new GameModeChangedEvent(currentSelectedModeInfo));
    }
    
    public void SaveLastPlayedGauntletMode()
    {
        if ((int)CurrentSelectedModeInfo.gameMode < gameModeInfo.gameModes.Length)
        {
            SaveManager.SetLastPlayedGauntletMode((int)CurrentSelectedModeInfo.gameMode);
        }
    }
    
    public void StartGame()
    {
        if (selectedLevel <= 0)
        {
            EventBus.Publish(new PromptToSelectLevelEvent());
            return;
        }

        SaveLastPlayedGauntletMode();
        SceneManager.LoadScene(currentSelectedModeInfo.scene);
    }

    public int GetNumPlayers()
    {
        return currentNumPlayers;
    }

    public int GetSelectedLevel()
    {
        return selectedLevel;
    }

    public GameModeType GetSelectedGameMode()
    {
        return currentSelectedMode;
    }

    public void SomeButtonClicked(ButtonClickedEvent e)
    {
        if (e.ButtonGroup == ButtonGroup.LevelSelection)
        {
            LevelSelection(e.Index);
        }
    }

    public void LevelSelection(int _SelectedLevel)
    {
        selectedLevel = _SelectedLevel;
    }

    public void SetNextLevelSelected()
    {
        selectedLevel++;
    }

    public void ResetSelectedLevel()
    {
        selectedLevel = -1;
    }
    
    public bool IsNextLevelAvailable()
    {
        return GameModeLevelMapping.Instance.GetNumLevelsInGameMode(currentSelectedMode) > GetSelectedLevel();
    }

    public bool IsNextLevelUnlocked()
    {
        return SaveManager.GetHighestClearedLevel(GetSelectedGameMode()) >= GetSelectedLevel();
    }
    
    public bool IsNextLevelAvailableAndUnlocked()
    {
        return IsNextLevelAvailable() && IsNextLevelUnlocked();
    }

    public int GetEndlessLevel()
    {
        return gameModeInfo.GetEndlessLevel();
    }

    public int GetTutorialLevel()
    {
        return gameModeInfo.GetTutorialLevel();
    }

    public void EndlessModeOpened()
    {
        currentSelectedModeInfo = endlessGameMode;
    }

    public bool IsPlayingEndlessMode()
    {
        return CurrentSelectedModeInfo.gameMode == GameModeType.Endless;
    }

    public void LevelSelectionMenuOpened()
    {
        if (SaveManager.IsSaveLoaded)
        {
            GameModeSelected((GameModeType)SaveManager.GetLastPlayedGauntletMode());
        }
        ResetSelectedLevel();
    }
    
#region Endless Mode

    public EndlessGenerationSettings EndlessGenerationSettings;
    public void SetRandomizerGenerationSettings(EndlessGenerationSettings settings)
    {
        EndlessGenerationSettings = settings;
    }

    public int GetEndlessModeDifficulty()
    {
        return (int)EndlessGenerationSettings[RandomizerParameterType.Dificulty];
    }

#endregion
}
