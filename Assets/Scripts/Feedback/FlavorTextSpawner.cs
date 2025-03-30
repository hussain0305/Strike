using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FlavorTextSpawner : MonoBehaviour
{
    [SerializeField] private GameObject flavorPrefab;
    [SerializeField] private List<Sprite> positiveSprites;
    [SerializeField] private List<Sprite> negativeSprites;
    [SerializeField] private int poolSize = 10;

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject ft = Instantiate(flavorPrefab, transform);
            ft.SetActive(false);
            pool.Enqueue(ft);
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe<CollectibleHitEvent>(CollectibleHit);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<CollectibleHitEvent>(CollectibleHit);
    }

    public void CollectibleHit(CollectibleHitEvent e)
    {
        Spawn(e.HitPosition, e.Value > 0);
    }

    public void Spawn(Vector3 worldPos, bool isPositive)
    {
        Sprite chosenSprite = GetRandomSprite(isPositive);
        GameObject ft = GetFromPool();

        ft.transform.position = worldPos;
        ft.SetActive(true);
        ft.GetComponent<FlavorText>().Init(chosenSprite, this);
    }

    public void ReturnToPool(GameObject ft)
    {
        ft.gameObject.SetActive(false);
        pool.Enqueue(ft);
    }

    private GameObject GetFromPool()
    {
        if (pool.Count > 0)
            return pool.Dequeue();

        GameObject ft = Instantiate(flavorPrefab, transform);
        ft.SetActive(false);
        return ft;
    }

    private Sprite GetRandomSprite(bool isPositive)
    {
        var list = isPositive ? positiveSprites : negativeSprites;
        return list.Count > 0 ? list[Random.Range(0, list.Count)] : null;
    }
}
