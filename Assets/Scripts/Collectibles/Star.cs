using UnityEngine;

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

public class Star : MonoBehaviour
{
    public int index;

    private int collectionCollisionMask;

    private void Start()
    {
        collectionCollisionMask = LayerMask.GetMask("OtherCollectingObject", "Ball");
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((collectionCollisionMask & (1 << other.gameObject.layer)) != 0)
        {
            GameManager.Instance.StarCollected(index);
            EventBus.Publish(new StarCollectedEvent(index, other.transform.position));
            gameObject.SetActive(false);
        }
    }
}
