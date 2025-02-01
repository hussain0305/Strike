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

    public BallProperties GetBall(int index)
    {
        if (index < allBalls.Count)
        {
            return allBalls[index];
        }

        return allBalls[0];
    }
}
