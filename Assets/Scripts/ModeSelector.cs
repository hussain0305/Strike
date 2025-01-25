using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ModeSelector : MonoBehaviour
{
    [Header("Game Mode")]
    public GameModeCollection gameModeInfo;
    public Button nextGameModeButton;
    public Button prevGameModeButton;
    public TextMeshProUGUI selectedGameModeNameText;
    public TextMeshProUGUI selectedGameModeDescriptionText;
    public Transform levelGrid;
    private GameModeType currentSelectedMode;
    private GameModeInfo currentSelectedModeInfo;

    [Header("Levels")]
    public GameModeLevelMapping levelMapping;
    public Transform levelButtonParent;
    public LevelSelectionButton levelButtonPrefab;
    private List<LevelSelectionButton> levelButtonsPool = new List<LevelSelectionButton>();
    
    [Header("Players and Play")]
    public Button addPlayerButton;
    public Button removePlayerButton;
    public Button playButton;    
    public TextMeshProUGUI currentNumPlayersText;
    private int maxPlayers = 8;
    
    private int currentNumPlayers = 1;
    private int selectedLevel = 1;
    
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
    }

    private void OnDisable()
    {
        addPlayerButton?.onClick.RemoveAllListeners();
        removePlayerButton?.onClick.RemoveAllListeners();
        playButton?.onClick.RemoveAllListeners();
    }

    private void Start()
    {
        GameModeSelected(GameModeType.Pins);
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
        selectedLevel = -1;
        currentSelectedMode = currentSelected;
        currentSelectedModeInfo = gameModeInfo.GetGameModeInfo(currentSelected);
        selectedGameModeNameText.text = currentSelectedModeInfo.displayName;
        selectedGameModeDescriptionText.text = currentSelectedModeInfo.description;
        PopulateLevelButtons();
    }

    private void PopulateLevelButtons()
    {
        foreach (var button in levelButtonsPool)
        {
            button.gameObject.SetActive(false);
        }
        
        var levels = levelMapping.GetLevelsForGameMode(currentSelectedMode);
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
        }

        for (int i = levels.Count; i < levelButtonsPool.Count; i++)
        {
            levelButtonsPool[i].gameObject.SetActive(false);
        }
    }
    
    public void StartGame()
    {
        if (currentSelectedMode == null || selectedLevel <= 0)
        {
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

    public void SetSelectedLevel(int level)
    {
        selectedLevel = level;
        HighlightSelectedButton();
    }

    public void HighlightSelectedButton()
    {
        foreach (LevelSelectionButton lsb in levelButtonsPool)
        {
            if (!lsb.gameObject.activeSelf)
            {
                continue;
            }
            if (lsb.LevelNumber == selectedLevel)
            {
                lsb.SetBorderMaterial(GlobalAssets.Instance.GetSelectedMaterial(ButtonLocation.MainMenu));
            }
            else
            {
                lsb.SetBorderMaterial(GlobalAssets.Instance.GetDefaultMaterial(ButtonLocation.MainMenu));
            }
        }
    }
}
