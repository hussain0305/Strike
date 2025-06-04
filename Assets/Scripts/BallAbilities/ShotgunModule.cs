using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunModule : IBallAbilityModule
{
    Ball ball;
    IContextProvider context;

    private Queue<GameObject> pelletPool = new Queue<GameObject>();
    private List<GameObject> activePellets = new List<GameObject>();
    private int pelletCount;
    private int pelletsToFire;
    private Vector2 spread;
    private GameObject pelletPrefab;

    public AbilityAxis Axis => AbilityAxis.Spawn;

    public ShotgunModule(GameObject _pellePrefab, Vector2 _spread, int _poolSize, int _pelletsToFire)
    {
        pelletPrefab = _pellePrefab;
        spread = _spread;
        pelletCount = _poolSize;
        pelletsToFire = _pelletsToFire;
    }
    
    public void Initialize(Ball ownerBall, IContextProvider _context)
    {
        ball = ownerBall;
        context = _context;

        ball.StartCoroutine(InitializePelletPool());
    }

    public void OnBallShot(BallShotEvent e)
    {
        FireShotgunPellets();
    }

    public void OnProjectilesSpawned(ProjectilesSpawnedEvent e) { }
    public void OnHitSomething(BallHitSomethingEvent e) { }

    public void OnNextShotCued(NextShotCuedEvent e)
    {
        foreach (GameObject pellet in activePellets)
        {
            ReturnPelletToPool(pellet);
        }
        activePellets.Clear();
    }
    
    private IEnumerator InitializePelletPool()
    {
        pelletPool = new Queue<GameObject>();
        for (int i = 0; i < pelletCount; i++)
        {
            GameObject pellet = Object.Instantiate(pelletPrefab);
            pellet.SetActive(false);
            pelletPool.Enqueue(pellet);
            yield return null;
        }
        EventBus.Publish(new ProjectilesSpawnedEvent(pelletPool.ToArray()));
    }

    private GameObject GetPelletFromPool()
    {
        if (pelletPool.Count > 0)
        {
            return pelletPool.Dequeue();
        }
        return Object.Instantiate(pelletPrefab, ball.transform);
    }

    public void ReturnPelletToPool(GameObject pellet)
    {
        pellet.SetActive(false);
        pelletPool.Enqueue(pellet);
    }
    
    private void FireShotgunPellets()
    {
        Transform aim = context.GetAimTransform();
        for (int i = 0; i < pelletsToFire; i++)
        {
            GameObject pellet = GetPelletFromPool();
            if (pellet != null)
            {
                pellet.transform.position = ball.transform.position;
                pellet.transform.rotation = aim.rotation;
                pellet.SetActive(true);
                activePellets.Add(pellet);
                
                Vector3 spreadOffset = (aim.right * Random.Range(-spread.x, spread.x)) +
                                       (aim.up * Random.Range(-spread.y, spread.y));
                
                Rigidbody rb = pellet.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = (aim.forward + spreadOffset) * context.GetLaunchForce();
                }
            }
        }
    }

    public void Cleanup()
    {
        GameObject[] pelletArray = pelletPool.ToArray();
        for (int i = 0; i < pelletArray.Length; i++)
        {
            GameObject.Destroy(pelletArray[i]);   
        }
    }
}