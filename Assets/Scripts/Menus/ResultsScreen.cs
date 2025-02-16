using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultsScreen : MonoBehaviour
{
    [Header("Win Condition - Points Required")]
    public Transform winConditionPointsRequirement;
    public Transform wonMessage;
    public Transform lostMessage;
    
    [Header("Win Condition - Points Ranking")]
    public Transform winConditionPointsRanking;
    public RankRow[] rankRows;
    
    [Header("Universal")]
    public Button mainMenuButton;
    public Button retryButton;
    public Button nextLevelButton;
    
    private static ResultsScreen instance;
    public static ResultsScreen Instance => instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
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
        
        switch (GameMode.Instance.GetWinCondition())
        {
            case WinCondition.PointsRanking:
                ShowPointsRankingResultScreen();
                break;
            case WinCondition.PointsRequired:
                ShowPointsRequiredResultScreen();
                break;
        }

        CheckIsNextLevelAvailable();
    }

    public void ShowPointsRequiredResultScreen()
    {
        winConditionPointsRequirement.gameObject.SetActive(true);
        int playerPoints = RoundDataManager.Instance.GetPointsForPlayer(0);
        bool levelCleared = playerPoints >= GameMode.Instance.pointsRequired;
        wonMessage.gameObject.SetActive(levelCleared);
        lostMessage.gameObject.SetActive(!levelCleared);
        if (levelCleared)
        {
            SaveManager.SetLevelCompleted(ModeSelector.Instance.GetSelectedGameMode(), ModeSelector.Instance.GetSelectedLevel());
        }
    }

    public void ShowPointsRankingResultScreen()
    {
        winConditionPointsRanking.gameObject.SetActive(true);
        List<PlayerGameData> playerRanks = RoundDataManager.Instance.GetPlayerRankings();
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
        GameStateManager.Instance.RetryLevel();
    }
    
    public void MainMenuButtonClicked()
    {
        GameStateManager.Instance.ReturnToMainMenu();
    }
    
    public void NextLevelButtonClicked()
    {
        GameStateManager.Instance.LoadNextLevel();
    }

    public void CheckIsNextLevelAvailable()
    {
        bool showNextLevelButton = ModeSelector.Instance.IsNextLevelAvailableAndUnlocked();
        nextLevelButton.transform.parent.gameObject.SetActive(showNextLevelButton);
    }
}
