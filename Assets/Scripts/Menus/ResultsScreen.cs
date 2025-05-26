using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ResultsScreen : MonoBehaviour
{
    [Header("Win Condition - Points Required")]
    public Transform winConditionPointsRequirement;
    public Transform wonMessage;
    public Transform lostMessage;
    
    [Header("Win Condition - Points Ranking")]
    public Transform winConditionPointsRanking;
    public TextMeshProUGUI winConditionPointsRankingText;
    public RankRow[] rankRows;
    
    [Header("Universal")]
    public Button mainMenuButton;
    public Button retryButton;
    public Button nextLevelButton;
    
    private ModeSelector modeSelector;
    private GameStateManager gameStateManager;
    private RoundDataManager roundDataManager;
    private GameMode gameMode;
    
    [Inject]
    public void Construct(ModeSelector _modeSelector, GameStateManager _gameStateManager, RoundDataManager _roundDataManager, GameMode _gameMode)
    {
        modeSelector = _modeSelector;
        gameStateManager = _gameStateManager;
        roundDataManager = _roundDataManager;
        gameMode = _gameMode;
    }
    
    private void OnEnable()
    {
        retryButton.onClick.AddListener(RetryButtonClicked);
        mainMenuButton.onClick.AddListener(MainMenuButtonClicked);
        nextLevelButton.onClick.AddListener(NextLevelButtonClicked);
    }

    private void OnDisable()
    {
        retryButton.onClick.RemoveAllListeners();
        mainMenuButton.onClick.RemoveAllListeners();
        nextLevelButton.onClick.RemoveAllListeners();
    }


    public void SetupResults()
    {
        winConditionPointsRanking.gameObject.SetActive(false);
        winConditionPointsRequirement.gameObject.SetActive(false);
        
        switch (gameMode.GetWinCondition())
        {
            case WinCondition.PointsRanking:
                ShowMultiplayerResultScreen();
                break;
            case WinCondition.PointsRequired:
            case WinCondition.Survival:
                ShowSoloRulesResultScreen();
                break;
        }

        CheckIsNextLevelAvailable();
    }

    public void ShowSoloRulesResultScreen()
    {
        winConditionPointsRequirement.gameObject.SetActive(true);
        int playerPoints = roundDataManager.GetPointsForPlayer(0);
        bool levelCleared = playerPoints >= gameMode.PointsRequired;
        wonMessage.gameObject.SetActive(levelCleared);
        lostMessage.gameObject.SetActive(!levelCleared);
    }

    public void ShowMultiplayerResultScreen()
    {
        winConditionPointsRanking.gameObject.SetActive(true);
        List<PlayerGameData> playerRanks = roundDataManager.GetPlayerRankings();
        winConditionPointsRankingText.text = $"{playerRanks[0].name} wins";
        int i = 0;
        while (i < playerRanks.Count)
        {
            rankRows[i].gameObject.SetActive(true);
            rankRows[i].SetInfo((i + 1), playerRanks[i].name, playerRanks[i].totalPoints);
            i++;
        }

        while (i < rankRows.Length)
        {
            rankRows[i].gameObject.SetActive(false);
            i++;
        }
    }

    public void RetryButtonClicked()
    {
        //TODO: Reload current scene cannot be hardcoded
        gameStateManager.RetryLevel();
    }
    
    public void MainMenuButtonClicked()
    {
        gameStateManager.ReturnToMainMenu();
    }
    
    public void NextLevelButtonClicked()
    {
        gameStateManager.LoadNextLevel();
    }

    public void CheckIsNextLevelAvailable()
    {
        bool showNextLevelButton = modeSelector.IsNextLevelAvailableAndUnlocked() && !modeSelector.IsGauntletMode();
        nextLevelButton.transform.parent.gameObject.SetActive(showNextLevelButton);
    }
}
