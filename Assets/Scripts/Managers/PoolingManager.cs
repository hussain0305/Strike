using UnityEngine;
using System.Collections.Generic;

public class PoolingManager : MonoBehaviour
{
    private static PoolingManager instance;
    public static PoolingManager Instance => instance;
    
    public CollectiblePrefabMapping prefabMapping;

    private Dictionary<PointTokenType, Queue<GameObject>> pointTokensDictionary = new Dictionary<PointTokenType, Queue<GameObject>>();
    private Dictionary<MultiplierTokenType, Queue<GameObject>> multiplierTokensDictionary = new Dictionary<MultiplierTokenType, Queue<GameObject>>();
    private Queue<GameObject> starPool = new Queue<GameObject>();
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private GameObject GetObjectFromDictionary<T>(Dictionary<T, Queue<GameObject>> dictionary, T type, System.Func<T, GameObject> prefabGetter)
    {
        if (dictionary.TryGetValue(type, out Queue<GameObject> queue) && queue.Count > 0)
        {
            GameObject obj = queue.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        GameObject newObj = Instantiate(prefabGetter(type));
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

    public void ReturnObject(PointTokenType type, GameObject obj)
    {
        ReturnObjectToDictionary(pointTokensDictionary, type, obj);
    }

    public void ReturnObject(MultiplierTokenType type, GameObject obj)
    {
        ReturnObjectToDictionary(multiplierTokensDictionary, type, obj);
    }
    
    public GameObject GetStar()
    {
        if (starPool.Count > 0)
        {
            GameObject star = starPool.Dequeue();
            star.SetActive(true);
            return star;
        }

        return Instantiate(prefabMapping.GetStarPrefab());
    }

    public void ReturnStar(GameObject star)
    {
        star.transform.SetParent(transform);
        star.SetActive(false);
        starPool.Enqueue(star);
    }
}
