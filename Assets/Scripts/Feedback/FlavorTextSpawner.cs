using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class FlavorTextSpawner : MonoBehaviour
{
    [SerializeField] private GameObject flavorPrefab;
    [SerializeField] private string[] positiveMessages;
    [SerializeField] private string[] negativeMessages;
    [SerializeField] private Material[] positiveMaterials;
    [SerializeField] private Material[] negativeMaterials;
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
        Spawn(e.HitPosition, e.Value);
    }

    public void Spawn(Vector3 worldPos, int value)
    {
        bool isPositive = value > 0;
        float jitterStrength = 0;
        if (value > 15)
        {
            jitterStrength = Mathf.Lerp(0, 25, (float)value / 100);
        }
        GetRandomMessageAndMaterial(isPositive, out string chosenMessage, out Material chosenMaterial);
        GameObject ft = GetFromPool();

        ft.transform.position = worldPos;
        ft.SetActive(true);
        ft.GetComponent<FlavorText>().Init(chosenMessage, chosenMaterial, jitterStrength, this);
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

    private void GetRandomMessageAndMaterial(bool isPositive, out string msg, out Material mat)
    {
        var msgList = isPositive ? positiveMessages : negativeMessages;
        var matList = isPositive ? positiveMaterials : negativeMaterials;
        msg = msgList.Length > 0 ? msgList[Random.Range(0, msgList.Length)] : null;
        mat = matList.Length > 0 ? matList[Random.Range(0, matList.Length)] : null;
    }
}
