using System;
using UnityEngine;
using System.Collections.Generic;
using Zenject;

public class PoolingManager : MonoBehaviour
{
    [Inject]
    DiContainer diContainer;
    
    private CollectiblePrefabMapping prefabMapping => CollectiblePrefabMapping.Instance;

    private Dictionary<PointTokenType, Queue<GameObject>> pointTokensDictionary = new Dictionary<PointTokenType, Queue<GameObject>>();
    private Dictionary<MultiplierTokenType, Queue<GameObject>> multiplierTokensDictionary = new Dictionary<MultiplierTokenType, Queue<GameObject>>();
    private Dictionary<DangerTokenType, Queue<GameObject>> dangerTokensDictionary = new Dictionary<DangerTokenType, Queue<GameObject>>();
    private Dictionary<ObstacleType, Queue<GameObject>> obstaclesDictionary = new Dictionary<ObstacleType, Queue<GameObject>>();
    private Queue<GameObject> starPool = new Queue<GameObject>();

    private GameObject GetObjectFromDictionary<T>(Dictionary<T, Queue<GameObject>> dictionary, T type, System.Func<T, GameObject> prefabGetter)
    {
        if (dictionary.TryGetValue(type, out Queue<GameObject> queue) && queue.Count > 0)
        {
            GameObject obj = queue.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        GameObject newObj = Instantiate(prefabGetter(type));
        diContainer.InjectGameObject(newObj);
        return newObj;
    }

    private void ReturnObjectToDictionary<T>(Dictionary<T, Queue<GameObject>> dictionary, T type, GameObject obj)
    {
        obj.transform.SetParent(transform);
        obj.SetActive(false);
        if (!dictionary.TryGetValue(type, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            dictionary[type] = queue;
        }
        queue.Enqueue(obj);
    }

    public GameObject GetObject(PointTokenType pointTokenType)
    {
        return GetObjectFromDictionary(pointTokensDictionary, pointTokenType, prefabMapping.GetPointTokenPrefab);
    }

    public GameObject GetObject(MultiplierTokenType multiplierTokenType)
    {
        return GetObjectFromDictionary(multiplierTokensDictionary, multiplierTokenType, prefabMapping.GetMultiplierTokenPrefab);
    }

    public GameObject GetObject(DangerTokenType dangerTokenType)
    {
        return GetObjectFromDictionary(dangerTokensDictionary, dangerTokenType, prefabMapping.GetDangerTokenPrefab);
    }

    public GameObject GetObject(ObstacleType obstacleType)
    {
        return GetObjectFromDictionary(obstaclesDictionary, obstacleType, prefabMapping.GetObstaclePrefab);
    }

    public void ReturnObject(PointTokenType type, GameObject obj)
    {
        ReturnObjectToDictionary(pointTokensDictionary, type, obj);
    }

    public void ReturnObject(MultiplierTokenType type, GameObject obj)
    {
        ReturnObjectToDictionary(multiplierTokensDictionary, type, obj);
    }
    
    public void ReturnObject(DangerTokenType type, GameObject obj)
    {
        ReturnObjectToDictionary(dangerTokensDictionary, type, obj);
    }
    
    public void ReturnObject(ObstacleType type, GameObject obj)
    {
        ReturnObjectToDictionary(obstaclesDictionary, type, obj);
    }
    
    public GameObject GetStar()
    {
        if (starPool.Count > 0)
        {
            GameObject star = starPool.Dequeue();
            star.SetActive(true);
            return star;
        }

        GameObject newStar = Instantiate(prefabMapping.GetStarPrefab());
        diContainer.InjectGameObject(newStar);

        return newStar;
    }

    public void ReturnStar(GameObject star)
    {
        star.transform.SetParent(transform);
        star.SetActive(false);
        starPool.Enqueue(star);
    }
}
