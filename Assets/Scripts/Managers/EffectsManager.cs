using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class EffectsManager : MonoBehaviour
{
    public GameObject flatEffectPrefab;
    public GameObject pfxPrefabCollectibleHit;
    public GameObject pfxPrefabObstacleHit;
    public GameObject pfxPrefabDangerHit;
    public GameObject starHitPrefab;

    private Queue<GameObject> flatEffectPool = new Queue<GameObject>();
    private Queue<GameObject> starPFXPool = new Queue<GameObject>();
    private Queue<GameObject> pfxCollectibleHitPool = new Queue<GameObject>();
    private Queue<GameObject> pfxObstacleHitPool = new Queue<GameObject>();
    private Queue<GameObject> pfxDangerHitPool = new Queue<GameObject>();

    private void OnEnable()
    {
        EventBus.Subscribe<BallHitSomethingEvent>(HandleBallHit);
        EventBus.Subscribe<StarCollectedEvent>(StarCollected);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<BallHitSomethingEvent>(HandleBallHit);
        EventBus.Unsubscribe<StarCollectedEvent>(StarCollected);
    }

    private void HandleBallHit(BallHitSomethingEvent e)
    {
        Vector3 hitPosition = e.Collision.GetContact(0).point;
        Vector3 normal = e.Collision.GetContact(0).normal;
        
        if (e.PfxTypes.Contains(PFXType.FlatHitEffect))
        {
            PlayFlatEffect(hitPosition, normal, e.Collision.gameObject.transform);
        }

        if (e.PfxTypes.Contains(PFXType.HitPFXCollectible))
        {
            PlayPFXHitCollectible(hitPosition);
        }
        
        if (e.PfxTypes.Contains(PFXType.HitPFXObstacle))
        {
            PlayPFXHitObstacle(hitPosition);
        }
        
        if (e.PfxTypes.Contains(PFXType.HitPFXDanger))
        {
            PlayPFXHitDanger(hitPosition);
        }
    }

    private void PlayFlatEffect(Vector3 position, Vector3 normal, Transform newParent)
    {
        GameObject effect = GetFromPool(flatEffectPool, flatEffectPrefab);
        FlatHitEffect hitEffect = effect.GetComponentInChildren<FlatHitEffect>();
        hitEffect.ActivateHitEffect();
        effect.transform.parent = null;
        effect.transform.position = position;
        effect.transform.rotation = Quaternion.LookRotation(normal, Vector3.forward);
        effect.transform.localScale = Vector3.one;
        effect.transform.parent = newParent;
        effect.SetActive(true);
        ReturnToPool(effect, flatEffectPool, 1.5f);
    }

    private void PlayPFXHitCollectible(Vector3 position)
    {
        GameObject effect = GetFromPool(pfxCollectibleHitPool, pfxPrefabCollectibleHit);
        effect.transform.position = position;
        effect.SetActive(true);

        ReturnToPool(effect, pfxCollectibleHitPool, 1f);
    }

    private void PlayPFXHitObstacle(Vector3 position)
    {
        GameObject effect = GetFromPool(pfxObstacleHitPool, pfxPrefabObstacleHit);
        effect.transform.position = position;
        effect.SetActive(true);

        ReturnToPool(effect, pfxObstacleHitPool, 1f);
    }

    private void PlayPFXHitDanger(Vector3 position)
    {
        GameObject effect = GetFromPool(pfxDangerHitPool, pfxPrefabDangerHit);
        effect.transform.position = position;
        effect.SetActive(true);

        ReturnToPool(effect, pfxDangerHitPool, 1f);
    }

    private void PlayStarHitPFX(Vector3 position)
    {
        GameObject effect = GetFromPool(starPFXPool, starHitPrefab);
        effect.transform.position = position;
        effect.SetActive(true);
        
        ReturnToPool(effect, pfxCollectibleHitPool, 1f);
    }

    private void StarCollected(StarCollectedEvent e)
    {
        PlayStarHitPFX(e.Position);
    }

    private GameObject GetFromPool(Queue<GameObject> pool, GameObject prefab)
    {
        if (pool.Count > 0)
        {
            return pool.Dequeue();
        }
        return Instantiate(prefab);
    }

    private void ReturnToPool(GameObject obj, Queue<GameObject> pool, float delay)
    {
        StartCoroutine(ReturnAfterDelay(obj, pool, delay));
    }

    private IEnumerator ReturnAfterDelay(GameObject obj, Queue<GameObject> pool, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
