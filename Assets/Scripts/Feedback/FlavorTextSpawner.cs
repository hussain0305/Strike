using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class FlavorTextSpawner : MonoBehaviour
{
    public GameObject flavorPrefab;
    public Material[] positiveMaterials;
    public Material[] negativeMaterials;
    public Material dangerMaterial;
    public int poolSize = 10;
    
    public readonly Dictionary<int, string[]> pointThresholdMessages = new Dictionary<int, string[]>
    {
        { -90, new[] { "Catastrophic!", "All downhill…", "That was rough!", "I am never going to financially recover from this!" } },
        { -60, new[] { "Brutal!", "Devastating!", "Oopsie!"} },
        { -30, new[] { "Rough!", "Not great…", "Ugh.", "That stings!" } },
        {   0, new[] { "Oops!", "Dang", "Yikes", "Ouch…" } },

        {  30, new[] { "Nice!", "Good job!", "Sweet!", "Well done!" } },
        {  60, new[] { "Awesome!", "Great work!", "You’re on fire!", "Keep it up!" } },
        {  90, new[] { "Incredible!", "Fantastic!", "Unstoppable!", "Wowzer!" } },
        { 120, new[] { "Holy smokes!", "Legendary!", "Mind-blowing!", "Next level!" } }
    };
    private string[] eliminationMessages = { "Game Over!", "Ah Dang it!", "Defeated!", "Better luck next time!" };

    private bool playerEliminated = false;
    private int numHitsInThisShot = 0;
    
    private Queue<GameObject> pool = new Queue<GameObject>();
    private Dictionary<int, int> flavorTextPositionBuckets = new Dictionary<int, int>();
    
    private int bucketWidth = 10;
    private const float MIN_JITTER = 0.1f;
    private const float MAX_JITTER = 4.0f;
    private const float ELIMINATION_JITTER = 2f;

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
        EventBus.Subscribe<NewRoundStartedEvent>(NewRoundStarted);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<CollectibleHitEvent>(CollectibleHit);
        EventBus.Unsubscribe<NewRoundStartedEvent>(NewRoundStarted);
    }

    public void CollectibleHit(CollectibleHitEvent e)
    {
        switch (e.Type)
        {
            case CollectibleType.Points:
            {
                numHitsInThisShot++;
                
                if (playerEliminated)
                    return;

                float jitterStrength = 0;
                jitterStrength = Mathf.Lerp(MIN_JITTER, MAX_JITTER, (float)e.Value / 100);
                GetRandomMessageAndMaterial(e.Value, out string chosenMessage, out Material chosenMaterial);
                GameObject ft = GetFromPool();

                int bucketKey = GetXBucketKey(e.HitPosition.x);
                if (!flavorTextPositionBuckets.ContainsKey(bucketKey))
                    flavorTextPositionBuckets.Add(bucketKey, 0);
                flavorTextPositionBuckets[bucketKey]++;
                
                ft.transform.position = e.HitPosition + new Vector3(Random.Range(flavorTextPositionBuckets[bucketKey] - 2, flavorTextPositionBuckets[bucketKey] - 1), 
                    flavorTextPositionBuckets[bucketKey] * 4.5f, 0);
                ft.SetActive(true);
                ft.GetComponent<FlavorText>().Init(chosenMessage, chosenMaterial, jitterStrength, this);
                break;
            }
            case CollectibleType.Danger:
            {
                playerEliminated = true;
                float jitterStrength = ELIMINATION_JITTER;
                Material chosenMaterial = dangerMaterial;
                string chosenMessage = eliminationMessages[Random.Range(0, eliminationMessages.Length)];
                GameObject ft = GetFromPool();

                ft.transform.position = e.HitPosition;
                ft.SetActive(true);
                ft.GetComponent<FlavorText>().Init(chosenMessage, chosenMaterial, jitterStrength, this);

                break;
            }
        }
    }

    public void NewRoundStarted(NewRoundStartedEvent e)
    {
        playerEliminated = false;
        numHitsInThisShot = 0;
        flavorTextPositionBuckets.Clear();
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
    
    public static int GetTierKey(int points)
    {
        float tierBlock = points / 30f;
        int tierCount = (int)(tierBlock > 0 ? Mathf.Ceil(tierBlock) : Mathf.Floor(tierBlock));
        return tierCount * 30;
    }

    private void GetRandomMessageAndMaterial(int points, out string msg, out Material mat)
    {
        int key = GetTierKey(points);
        if (!pointThresholdMessages.ContainsKey(key))
        {
            var allKeys = new List<int>(pointThresholdMessages.Keys);
            allKeys.Sort();
            if (points < 0)
                key = allKeys[0];
            else
                key = allKeys[^1];
        }

        var messageOptions = pointThresholdMessages[key];
        msg = messageOptions[Random.Range(0, messageOptions.Length)];
        
        var matList = points > 0 ? positiveMaterials : negativeMaterials;
        mat = matList.Length > 0 ? matList[Random.Range(0, matList.Length)] : null;
    }
    
    public int GetXBucketKey(float x)
    {
        int bucketIndex = Mathf.FloorToInt(x / bucketWidth);
        return (bucketIndex * bucketWidth) + (bucketWidth / 2);
    }
}
