using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameModeChangedEvent { }

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
    public Button nextGameModeButton;
    public Button prevGameModeButton;
    public TextMeshProUGUI selectedGameModeNameText;
    public TextMeshProUGUI selectedGameModeDescriptionText;
    public GameObject bottomPanelGameModeUnlocked;

    [Header("Game Mode - Locked")]
    public GameObject lockedSection;
    public TextMeshProUGUI unlockRequirementText;
    public Button unlockGameModeButton;
    public GameObject bottomPanelGameModeLocked;

    [Header("Levels")]
    public Transform levelButtonParent;
    public LevelSelectionButton levelButtonPrefab;
    private List<LevelSelectionButton> levelButtonsPool = new List<LevelSelectionButton>();
    
    [Header("Players and Play")]
    public Button addPlayerButton;
    public Button removePlayerButton;
    public Button playButton;    
    public TextMeshProUGUI currentNumPlayersText;

    [Header("DEBUG")]
    public Button addStars;
    public Button deductStars;
    
    private int maxPlayers = 8;
    
    private int currentNumPlayers = 1;
    private int selectedLevel = 1;
    
    private GameModeInfo currentSelectedModeInfo;
    public GameModeInfo CurrentSelectedModeInfo => currentSelectedModeInfo;

    private GameModeType currentSelectedMode;
    public GameModeType CurrentSelectedMode => currentSelectedMode;

    private static ModeSelector instance;
    public static ModeSelector Instance => instance;
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(instance.gameObject);
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        nextGameModeButton?.onClick.AddListener(NextGameMode);
        prevGameModeButton?.onClick.AddListener(PreviousGameMode);
        
        addPlayerButton?.onClick.AddListener(AddPlayer);
        removePlayerButton?.onClick.AddListener(RemovePlayer);
        
        playButton.onClick?.AddListener(StartGame);
        
        EventBus.Subscribe<ButtonClickedEvent>(SomeButtonClicked);
        
        //===TODO: DEBUG. DELETE LATER===
        addStars.onClick.AddListener(() =>
        {
            SaveManager.AddStars(40);
        });
        //===TODO: DEBUG. DELETE LATER===
        deductStars.onClick.AddListener(() =>
        {
            SaveManager.SpendStars(20);
        });
    }

    private void OnDisable()
    {
        addPlayerButton?.onClick.RemoveAllListeners();
        removePlayerButton?.onClick.RemoveAllListeners();
        playButton?.onClick.RemoveAllListeners();
        EventBus.Unsubscribe<ButtonClickedEvent>(SomeButtonClicked);
    }

    public void Init()
    {
        GameModeSelected((GameModeType)0);
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
        currentNumPlayersText.text = currentNumPlayers.ToString();
        addPlayerButton.enabled = true;
        removePlayerButton.enabled = true;
        
        if (currentNumPlayers <= 1)
        {
            removePlayerButton.enabled = false;
        }
        if (currentNumPlayers >= maxPlayers)
        {
            addPlayerButton.enabled = false;
        }
        EventBus.Publish(new NumPlayersChangedEvent(currentNumPlayers));
    }

    public void NextGameMode()
    {
        currentSelectedMode = (GameModeType)(((int)currentSelectedMode + 1) % System.Enum.GetValues(typeof(GameModeType)).Length);
        Debug.Log("Next Game Mode: " + currentSelectedMode);
        GameModeSelected(currentSelectedMode);
    }

    public void PreviousGameMode()
    {
        int totalModes = System.Enum.GetValues(typeof(GameModeType)).Length;
        currentSelectedMode = (GameModeType)(((int)currentSelectedMode - 1 + totalModes) % totalModes);
        Debug.Log("Previous Game Mode: " + currentSelectedMode);
        GameModeSelected(currentSelectedMode);
    }

    public void GameModeSelected(GameModeType currentSelected)
    {
        EventBus.Publish(new GameModeChangedEvent());
        ResetSelectedLevel();
        currentSelectedMode = currentSelected;
        currentSelectedModeInfo = gameModeInfo.GetGameModeInfo(currentSelected);
        selectedGameModeNameText.text = currentSelectedModeInfo.displayName;
        selectedGameModeDescriptionText.text = currentSelectedModeInfo.description;

        if (SaveManager.GetIsGameModeUnlocked((int)currentSelected))
        {
            SetupUnlockedGameModePanel();
        }
        else
        {
            SetupLockedGameModePanel();
        }
    }

    private void SetupLockedGameModePanel()
    {
        lockedSection.gameObject.SetActive(true);
        bottomPanelGameModeLocked.SetActive(true);
        bottomPanelGameModeUnlocked.SetActive(false);
        levelButtonParent.gameObject.SetActive(false);

        int currentStars = SaveManager.GetStars();
        int requiredStars = gameModeInfo.GetGameModeInfo(currentSelectedMode).starsRequiredToUnlock;

        unlockRequirementText.text = $"{requiredStars} STARS REQUIRED TO UNLOCK THIS GAME MODE";
        
        if (currentStars >= requiredStars)
        {
            unlockGameModeButton.gameObject.SetActive(true);
            unlockGameModeButton.onClick.RemoveAllListeners();
            unlockGameModeButton.onClick.AddListener(() =>
            {
                SaveManager.SpendStars(requiredStars);
                SaveManager.SetGameModeUnlocked((int)currentSelectedMode);
                SetupUnlockedGameModePanel();
            });
        }
        else
        {
            unlockGameModeButton.onClick.RemoveAllListeners();
            unlockGameModeButton.gameObject.SetActive(false);
        }
    }

    private void SetupUnlockedGameModePanel()
    {
        lockedSection.gameObject.SetActive(false);
        bottomPanelGameModeLocked.SetActive(false);
        bottomPanelGameModeUnlocked.SetActive(true);
        levelButtonParent.gameObject.SetActive(true);

        foreach (var button in levelButtonsPool)
        {
            button.gameObject.SetActive(false);
        }
        
        var levels = GameModeLevelMapping.Instance.GetLevelsForGameMode(currentSelectedMode);
        int highestClearedLevel = SaveManager.GetHighestClearedLevel(currentSelectedMode);
        while (levelButtonsPool.Count < levels.Count)
        {
            LevelSelectionButton newButton = Instantiate(levelButtonPrefab, levelButtonParent);
            newButton.gameObject.SetActive(false);
            levelButtonsPool.Add(newButton);
        }

        for (int i = 0; i < levels.Count; i++)
        {
            LevelSelectionButton button = levelButtonsPool[i];
            button.gameObject.SetActive(true);
            button.SetMappedLevel(currentSelectedMode, levels[i]);
            if (levels[i] <= highestClearedLevel + 1) //The next level after the highest unlocked will also be unlocked
            {
                button.SetUnlocked();
            }
            else
            {
                button.SetLocked();
            }
        }

        for (int i = levels.Count; i < levelButtonsPool.Count; i++)
        {
            levelButtonsPool[i].gameObject.SetActive(false);
        }
    }
    
    public void StartGame()
    {
        if (selectedLevel <= 0)
        {
            LevelSelectionMenu.Instance.PromptToSelectLevel();
            return;
        }

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

    public int GetTutorialLevel()
    {
        return gameModeInfo.GetTutorialLevel();
    }
}
