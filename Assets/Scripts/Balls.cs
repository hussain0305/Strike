using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Balls", menuName = "Game/Balls")]
public class Balls : ScriptableObject
{
    private static Balls _instance;
    public static Balls Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<Balls>("Balls");
                if (_instance == null)
                {
                    Debug.LogError("Balls instance not found.");
                }
            }
            return _instance;
        }
    }

    public List<BallProperties> allBalls;
    public float maxWeight = 10;
    public float maxSpin = 15;
    public float maxBounce = 3;

    private Dictionary<string, BallProperties> ballProperties;
    
    public BallProperties GetBall(string id)
    {
        if (ballProperties == null)
        {
            SetupDictionary();
        }

        return ballProperties.TryGetValue(id, out var properties) 
            ? properties 
            : new BallProperties { id = "unknown" };
    }

    public void SetupDictionary()
    {
        ballProperties = new Dictionary<string, BallProperties>();
        
        foreach (BallProperties ball in allBalls)
        {
            ballProperties.Add(ball.id, ball);
        }
    }

    public int GetBallCost(string id)
    {
        return GetBall(id).cost;
    }
}
