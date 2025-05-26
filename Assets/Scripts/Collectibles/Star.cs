using UnityEngine;
using Zenject;

public class StarCollectedEvent
{
    public int Index { get; }
    public Vector3 Position { get; }

    public StarCollectedEvent(int index, Vector3 position)
    {
        Index = index;
        Position = position;
    }
}

public class StarsEarnedEvent
{
    public int NumStarsEarned;
    public StarsEarnedEvent(int numStarsEarned)
    {
        NumStarsEarned = numStarsEarned;
    }
}

public class StarsSpentEvent
{
    public int NumStarsSpent;
    public StarsSpentEvent(int numStarsSpent)
    {
        NumStarsSpent = numStarsSpent;
    }
}

public class Star : MonoBehaviour
{
    public int index;

    private int collectionCollisionMask;

    private GameManager gameManager;

    [Inject]
    public void Construct(GameManager _gameManager)
    {
        gameManager = _gameManager;
    }

    private void Start()
    {
        collectionCollisionMask = LayerMask.GetMask("OtherCollectingObject", "Ball");
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((collectionCollisionMask & (1 << other.gameObject.layer)) != 0)
        {
            gameManager.StarCollected(index);
            EventBus.Publish(new StarCollectedEvent(index, other.transform.position));
            gameObject.SetActive(false);
        }
    }
}
