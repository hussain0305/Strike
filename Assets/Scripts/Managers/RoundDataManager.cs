using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerEliminatedEvent
{
    public int PlayerIndex;

    public PlayerEliminatedEvent(int playerIndex)
    {
        this.PlayerIndex = playerIndex;
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
    
    private static RoundDataManager instance;
    public static RoundDataManager Instance => instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private GameManager game;
    private GameManager Game => game ??= GameManager.Instance;
    
    private void OnEnable()
    {
        EventBus.Subscribe<BallShotEvent>(BallShot);
        EventBus.Subscribe<CollectibleHitEvent>(CollectibleHit);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<BallShotEvent>(BallShot);
        EventBus.Unsubscribe<CollectibleHitEvent>(CollectibleHit);
    }
    
    public void BallShot(BallShotEvent e)
    {
        PlayerGameData gameData = playerGameData[Game.CurrentPlayerTurn];
        gameData.shotsTaken++;
        playerGameData[Game.CurrentPlayerTurn] = gameData;
    }

    public void CreateNewPlayerRecord(int _index)
    {
        PlayerGameData data = new PlayerGameData();
        
        //TODO: Name might be introduced later
        data.name = $"Player {_index + 1}";
        data.totalPoints = 0;
        data.shotsTaken = 0;
        data.projectileViewsRemaining = GameMode.Instance.NumProjectileViews;
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
        PlayerGameData gameData = playerGameData[Game.CurrentPlayerTurn];
        PlayerScoreboard scoreboard = playerScoreboards[Game.CurrentPlayerTurn];

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
                EliminatePlayer(GameManager.Instance.CurrentPlayerTurn);
                break;
        }

        playerGameData[Game.CurrentPlayerTurn] = gameData;
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
        int currentShotTaker = Game.CurrentPlayerTurn;
        for (int i = 0; i < playerScoreboards.Keys.Count; i++)
        {
            playerScoreboards[i].SetCurrentShotTaker(i == currentShotTaker);
        }
    }
    
    public int GetTrajectoryViewsRemaining()
    {
        PlayerGameData gameData = playerGameData[Game.CurrentPlayerTurn];
        return gameData.projectileViewsRemaining;
    }

    public void TrajectoryViewUsed()
    {
        PlayerGameData gameData = playerGameData[Game.CurrentPlayerTurn];
        gameData.projectileViewsRemaining--;
        playerGameData[Game.CurrentPlayerTurn] = gameData;
    }

    public void AddPlayerShotHistory(int playerIndex, ShotInfo shotInfo)
    {
        PlayerGameData gameData = playerGameData[playerIndex];
        gameData.shotHistory.Add(shotInfo);
        playerGameData[playerIndex] = gameData;
    }

    public List<ShotInfo> GetTrajectoryHistory()
    {
        return playerGameData[Game.CurrentPlayerTurn].shotHistory;
    }
    
    public void StartLoggingShotInfo()
    {
        currentShotData.StartLogging(GameManager.Instance.CurrentPlayerTurn);
        currentShotInfo = new ShotInfo();
        currentShotInfo.angle = Game.AngleInput.CalculateProjectedAngle();
        currentShotInfo.spin = Game.SpinInput.SpinVector;
        currentShotInfo.power = (int)Game.PowerInput.Power;
    }

    public void FinishLoggingShotInfo(List<Vector3> capturedTrajectory)
    {
        currentShotInfo.points = currentShotData.pointsAccrued;
        currentShotInfo.trajectory = capturedTrajectory;
        AddPlayerShotHistory(currentShotData.ownerIndex, currentShotInfo);
        
        GameMode.Instance.OnShotComplete(currentShotData.hitDangerPin || currentShotData.hitNormalPint);
        
        currentShotData.Reset();
    }
    
    public void EliminatePlayer(int playerIndex)
    {
        if (!eliminationOrder.Contains(playerIndex))
        {
            eliminationOrder.Add(playerIndex);
            playerScoreboards[playerIndex].SetEliminated();
            EventBus.Publish(new PlayerEliminatedEvent(playerIndex));
        }
    }
}
