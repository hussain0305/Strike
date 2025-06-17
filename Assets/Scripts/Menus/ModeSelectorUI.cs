using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ModeSelectorUI : MonoBehaviour
{
    [Header("Game Mode")]
    public Button nextGameModeButton;
    public Button prevGameModeButton;
    public TextMeshProUGUI selectedGameModeNameText;
    public TextMeshProUGUI selectedGameModeDescriptionText;
    public GameObject bottomPanelGameModeUnlocked;
    public GameObject infoPanel;

    [Header("Game Mode - Locked")]
    public GameObject lockedSection;
    public TextMeshProUGUI unlockRequirementText;
    public Button unlockGameModeButton;
    public GameObject bottomPanelGameModeLocked;

    [Header("Levels")]
    public Transform levelButtonParent;
    public LevelSelectionButton levelButtonPrefab;
    private readonly List<LevelSelectionButton> levelButtonsPool = new List<LevelSelectionButton>();

    [Header("Players and Play")]
    public Button playButton;
    
    private ModeSelector modeSelector;

    [Inject]
    DiContainer diContainer;

    [Inject]
    public void Construct(ModeSelector _modeSelector)
    {
        modeSelector = _modeSelector;
    }

    private void OnEnable()
    {
        nextGameModeButton.onClick.AddListener(OnNextGameMode);
        prevGameModeButton.onClick.AddListener(OnPreviousGameMode);
        playButton.onClick.AddListener(OnPlay);
        
        EventBus.Subscribe<GameModeChangedEvent>(OnGameModeChanged);
        EventBus.Subscribe<NumPlayersChangedEvent>(OnNumPlayersChanged);
    }

    private void OnDisable()
    {
        nextGameModeButton.onClick.RemoveListener(OnNextGameMode);
        prevGameModeButton.onClick.RemoveListener(OnPreviousGameMode);
        playButton.onClick.RemoveListener(OnPlay);
        
        EventBus.Unsubscribe<GameModeChangedEvent>(OnGameModeChanged);
        EventBus.Unsubscribe<NumPlayersChangedEvent>(OnNumPlayersChanged);
    }

    // private void Start()
    // {
    //     UpdateGameModeUI();
    //     UpdateLevelButtons();
    // }

    private void OnNextGameMode()
    {
        modeSelector.NextGameMode();
    }

    private void OnPreviousGameMode()
    {
        modeSelector.PreviousGameMode();
    }

    private void OnPlay()
    {
        modeSelector.StartGame();
    }

    private void OnGameModeChanged(GameModeChangedEvent e)
    {
        UpdateGameModeUI();
        UpdateLevelButtons();
    }

    private void OnNumPlayersChanged(NumPlayersChangedEvent e)
    {
        
    }

    private void UpdateGameModeUI()
    {
        if (modeSelector.CurrentSelectedModeInfo.displayName == null)
        {
            return;
        }
        var info = modeSelector.CurrentSelectedModeInfo;
        selectedGameModeNameText.text = info.displayName.ToUpper();
        selectedGameModeDescriptionText.text = info.description;

        bool unlocked = SaveManager.GetIsGameModeUnlocked((int)modeSelector.CurrentSelectedMode);
        infoPanel.SetActive(unlocked);
        lockedSection.SetActive(!unlocked);
        bottomPanelGameModeLocked.SetActive(!unlocked);
        bottomPanelGameModeUnlocked.SetActive(unlocked);
        levelButtonParent.gameObject.SetActive(unlocked);

        if (!unlocked)
        {
            int requiredStars = info.starsRequiredToUnlock;
            unlockRequirementText.text = $"{requiredStars} STARS REQUIRED TO UNLOCK THIS GAME MODE";
            bool canAfford = SaveManager.GetStars() >= requiredStars;
            unlockGameModeButton.gameObject.SetActive(canAfford);
            unlockGameModeButton.onClick.RemoveAllListeners();
            if (canAfford)
                unlockGameModeButton.onClick.AddListener(() => {
                    SaveManager.SpendStars(requiredStars);
                    SaveManager.SetGameModeUnlocked((int)modeSelector.CurrentSelectedMode);
                    modeSelector.GameModeSelected(modeSelector.CurrentSelectedMode);
                });
        }
    }

    private void UpdateLevelButtons()
    {
        var info = modeSelector.CurrentSelectedModeInfo;
        var levels = GameModeLevelMapping.Instance.GetLevelsForGameMode(modeSelector.CurrentSelectedMode);
        int highestCleared = SaveManager.GetHighestClearedLevel(modeSelector.CurrentSelectedMode);

        while (levelButtonsPool.Count < levels.Count)
        {
            var btn = Instantiate(levelButtonPrefab, levelButtonParent);
            btn.gameObject.SetActive(false);
            diContainer.InjectGameObject(btn.gameObject);
            levelButtonsPool.Add(btn);
        }

        for (int i = 0; i < levels.Count; i++)
        {
            var button = levelButtonsPool[i];
            button.gameObject.SetActive(true);
            button.SetMappedLevel(modeSelector.CurrentSelectedMode, levels[i]);
            bool unlocked = levels[i] <= highestCleared + 1;
            if (unlocked) button.SetUnlocked(); else button.SetLocked();
        }

        for (int i = levels.Count; i < levelButtonsPool.Count; i++)
            levelButtonsPool[i].gameObject.SetActive(false);
    }
}
