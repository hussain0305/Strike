using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunModule : IBallAbilityModule
{
    //Interface stuff
    public AbilityAxis Axis => AbilityAxis.Spawn;
    public void Initialize(Ball ownerBall, IContextProvider _context)
    {
        ball = ownerBall;
        context  = _context;

        ball.StartCoroutine(InitializePelletPool());
    }

    public void OnBallShot(BallShotEvent e)
    {
        FireShotgunPellets();
    }

    public void OnProjectilesSpawned(ProjectilesSpawnedEvent e) { }
    public void OnHitSomething(BallHitSomethingEvent e) { }
    public void Cleanup() { }

    public void OnNextShotCued(NextShotCuedEvent e)
    {
        foreach (GameObject pellet in activePellets)
        {
            ReturnPelletToPool(pellet);
        }
        activePellets.Clear();
    }
    
    //Own stuff
    Ball ball;
    IContextProvider context;
    
    private Vector2 spread = new Vector2(0.5f, 0.5f);
    private Queue<GameObject> pelletPool = new Queue<GameObject>();
    private List<GameObject> activePellets = new List<GameObject>();
    private const int pelletCount = 50;

    private IEnumerator InitializePelletPool()
    {
        GameObject pelletPrefab = Balls.Instance.shotgunPellets;
        pelletPool = new Queue<GameObject>();
        for (int i = 0; i < pelletCount; i++)
        {
            GameObject pellet = Object.Instantiate(pelletPrefab, ball.transform);
            pellet.SetActive(false);
            pelletPool.Enqueue(pellet);
            yield return null;
        }
    }

    private GameObject GetPelletFromPool()
    {
        if (pelletPool.Count > 0)
        {
            return pelletPool.Dequeue();
        }
        return Object.Instantiate(Balls.Instance.shotgunPellets, ball.transform);
    }

    public void ReturnPelletToPool(GameObject pellet)
    {
        pellet.SetActive(false);
        pelletPool.Enqueue(pellet);
    }
    
    private void FireShotgunPellets()
    {
        Transform aim = context.GetAimTransform();
        int pelletsToFire = 20;
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
}