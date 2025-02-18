using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    public GameObject flatEffectPrefab;
    public GameObject pfx3DPrefab;
    public GameObject starHitPrefab;

    private Queue<GameObject> flatEffectPool = new Queue<GameObject>();
    private Queue<GameObject> pfx3DPool = new Queue<GameObject>();
    private Queue<GameObject> starPFXPool = new Queue<GameObject>();

    private void OnEnable()
    {
        Ball.OnBallHitSomething += HandleBallHit;
        Star.OnStarCollected += StarCollected;
    }

    private void OnDisable()
    {
        Ball.OnBallHitSomething -= HandleBallHit;
        Star.OnStarCollected -= StarCollected;
    }

    private void HandleBallHit(Collision collision, HashSet<PFXType> effectsToPlay)
    {
        Vector3 hitPosition = collision.GetContact(0).point;
        Vector3 normal = collision.GetContact(0).normal;
        
        if (effectsToPlay.Contains(PFXType.FlatHitEffect))
        {
            PlayFlatEffect(hitPosition, normal, collision.gameObject.transform);
        }

        if (effectsToPlay.Contains(PFXType.HitPFX3D))
        {
            PlayPFX3D(hitPosition);
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

    private void PlayPFX3D(Vector3 position)
    {
        GameObject effect = GetFromPool(pfx3DPool, pfx3DPrefab);
        effect.transform.position = position;
        effect.SetActive(true);

        ReturnToPool(effect, pfx3DPool, 1f);
    }

    private void PlayStarHitPFX(Vector3 position)
    {
        GameObject effect = GetFromPool(starPFXPool, starHitPrefab);
        effect.transform.position = position;
        effect.SetActive(true);
        
        ReturnToPool(effect, pfx3DPool, 1f);
    }

    private void StarCollected(int index, Vector3 position)
    {
        PlayStarHitPFX(position);
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
