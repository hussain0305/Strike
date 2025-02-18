using System;
using UnityEngine;

public class Star : MonoBehaviour
{
    public delegate void StarCollected(int index, Vector3 position);
    public static event StarCollected OnStarCollected;

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
            OnStarCollected?.Invoke(index, other.transform.position);
            gameObject.SetActive(false);
        }
    }
}
