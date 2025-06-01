using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class PlayerEliminatedEvent
{
    public int PlayerIndex;
    public EliminationReason EliminationReason;

    public PlayerEliminatedEvent(int playerIndex, EliminationReason eliminationReason)
    {
        this.PlayerIndex = playerIndex;
        this.EliminationReason = eliminationReason;
    }
}

public class RoundDataManager : MonoBehaviour
{
    public Canvas worldSpaceCanvas;
    public Transform collectibleHeadersParent;
    public CollectibleHeader collectibleHeaderPrefab;
    
    public PlayerScoreboard scoreboardPrefab;
    public Transform scoreboardParent;
    
    private Dictionary<int, PlayerGameData> playerGameData;
    private Dictionary<int, PlayerScoreboard> playerScoreboards;
    private List<int> eliminationOrder;
    public List<int> EliminationOrder => eliminationOrder;

    private ShotInfo currentShotInfo;
    private ShotData currentShotData;
    public  ShotData CurrentShotData => currentShotData;
    
    [Inject]
    private GameManager gameManager;
    [Inject]
    private GameMode gameMode;

    private void OnEnable()
    {
        EventBus.Subscribe<BallShotEvent>(BallShot);
        EventBus.Subscribe<ShotCompleteEvent>(ShotComplete);
        EventBus.Subscribe<CollectibleHitEvent>(CollectibleHit);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<BallShotEvent>(BallShot);
        EventBus.Unsubscribe<ShotCompleteEvent>(ShotComplete);
        EventBus.Unsubscribe<CollectibleHitEvent>(CollectibleHit);
    }
    
    public void BallShot(BallShotEvent e)
    {
        PlayerGameData gameData = playerGameData[gameManager.CurrentPlayerTurn];
        gameData.shotsTaken++;
        playerGameData[gameManager.CurrentPlayerTurn] = gameData;
    }

    public void ShotComplete(ShotCompleteEvent e)
    {
        FinishLoggingShotInfo(e.ShotTrajectory);
    }

    public void CreateNewPlayerRecord(int _index)
    {
        PlayerGameData data = new PlayerGameData();
        
        //TODO: Name might be introduced later
        data.name = $"Player {_index + 1}";
        data.totalPoints = 0;
        data.shotsTaken = 0;
        data.projectileViewsRemaining = gameMode.NumProjectileViews;
        data.shotHistory = new List<ShotInfo>();
        
        if (playerGameData == null)
        {
            playerGameData = new Dictionary<int, PlayerGameData>();
        }

        playerGameData.Add(_index, data);
    }

    public void CreatePlayers(int numPlayers)
    {
        eliminationOrder = new List<int>();
        playerScoreboards = new Dictionary<int, PlayerScoreboard>();
        for (int i = 0; i < numPlayers; i++)
        {
            CreateNewPlayerRecord(i);
            PlayerScoreboard scoreboard = Instantiate(scoreboardPrefab, scoreboardParent);
            scoreboard.SetPlayer(i);
            scoreboard.SetScore(0);
            playerScoreboards.Add(i, scoreboard);
        }
    }

    public void CollectibleHit(CollectibleHitEvent e)
    {
        PlayerGameData gameData = playerGameData[gameManager.CurrentPlayerTurn];
        PlayerScoreboard scoreboard = playerScoreboards[gameManager.CurrentPlayerTurn];

        switch (e.Type)
        {
            case CollectibleType.Multiple:
                currentShotData.multiplierAccrued *= e.Value;
                break;
            case CollectibleType.Points:
                int pointsFromThisHit = currentShotData.multiplierAccrued * e.Value;
                gameData.totalPoints += pointsFromThisHit;
                currentShotData.pointsAccrued += pointsFromThisHit;
                scoreboard.TickToScore(gameData.totalPoints, pointsFromThisHit);
                currentShotData.hitNormalPint = true;
                break;
            case CollectibleType.Danger:
                currentShotData.hitDangerPin = true;
                EliminatePlayer(gameManager.CurrentPlayerTurn, EliminationReason.HitDangerPin);
                break;
        }

        playerGameData[gameManager.CurrentPlayerTurn] = gameData;
    }

    public int GetPointsForPlayer(int index)
    {
        if (playerGameData.ContainsKey(index))
        {
            return playerGameData[index].totalPoints;
        }

        return -1;
    }
    
    public List<PlayerGameData> GetSortedPlayerData(List<PlayerGameData> players)
    {
        players.Sort((player1, player2) => player2.totalPoints.CompareTo(player1.totalPoints));
        return players;
    }
    
    public List<PlayerGameData> GetPlayerRankings()
    {
        List<PlayerGameData> finalList = new List<PlayerGameData>(playerGameData.Count);

        foreach (int idx in eliminationOrder)
        {
            var eliminated = playerGameData[idx];
            finalList.Add(eliminated);
        }

        var survivors = playerGameData
            .Where(kvp => !eliminationOrder.Contains(kvp.Key))
            .Select(kvp => kvp.Value)
            .OrderBy(p => p.totalPoints);

        foreach (var survivor in survivors)
        {
            finalList.Add(survivor);
        }

        finalList.Reverse();
        return finalList;
    }

    public void SetCurrentShotTaker()
    {
        int currentShotTaker = gameManager.CurrentPlayerTurn;
        for (int i = 0; i < playerScoreboards.Keys.Count; i++)
        {
            playerScoreboards[i].SetCurrentShotTaker(i == currentShotTaker);
        }
    }
    
    public int GetTrajectoryViewsRemaining()
    {
        PlayerGameData gameData = playerGameData[gameManager.CurrentPlayerTurn];
        return gameData.projectileViewsRemaining;
    }

    public void TrajectoryViewUsed()
    {
        PlayerGameData gameData = playerGameData[gameManager.CurrentPlayerTurn];
        gameData.projectileViewsRemaining--;
        playerGameData[gameManager.CurrentPlayerTurn] = gameData;
    }

    public void AddPlayerShotHistory(int playerIndex, ShotInfo shotInfo)
    {
        PlayerGameData gameData = playerGameData[playerIndex];
        gameData.shotHistory.Add(shotInfo);
        playerGameData[playerIndex] = gameData;
    }

    public List<ShotInfo> GetTrajectoryHistory()
    {
        return playerGameData[gameManager.CurrentPlayerTurn].shotHistory;
    }
    
    public void StartLoggingShotInfo()
    {
        currentShotData.StartLogging(gameManager.CurrentPlayerTurn);
        currentShotInfo = new ShotInfo();
        currentShotInfo.angle = gameManager.AngleInput.CalculateProjectedAngle();
        currentShotInfo.spin = gameManager.SpinInput.SpinVector;
        currentShotInfo.power = (int)gameManager.PowerInput.Power;
    }

    public void FinishLoggingShotInfo(List<Vector3> capturedTrajectory)
    {
        currentShotInfo.points = currentShotData.pointsAccrued;
        currentShotInfo.trajectory = capturedTrajectory;
        AddPlayerShotHistory(currentShotData.ownerIndex, currentShotInfo);
        
        gameMode.OnShotComplete(currentShotData.hitDangerPin, currentShotData.hitNormalPint);
        
        currentShotData.Reset();
    }
    
    public void EliminatePlayer(int playerIndex, EliminationReason reason)
    {
        if (!eliminationOrder.Contains(playerIndex))
        {
            eliminationOrder.Add(playerIndex);
            playerScoreboards[playerIndex].SetEliminated();
            EventBus.Publish(new PlayerEliminatedEvent(playerIndex, reason));
        }
    }
}
