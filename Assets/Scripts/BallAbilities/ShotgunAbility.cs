using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunAbility : BallAbility
{
    public GameObject pelletPrefab;
    private Vector2 spread = new Vector2(0.5f, 0.5f);
    private Queue<GameObject> pelletPool = new Queue<GameObject>();
    private List<GameObject> activePellets = new List<GameObject>();
    private const int pelletCount = 50;

    public override void Initialize(Ball ownerBall, IContextProvider _context)
    {
        base.Initialize(ownerBall, _context);
        StartCoroutine(InitializePelletPool());
    }
    

    private IEnumerator InitializePelletPool()
    {
        for (int i = 0; i < pelletCount; i++)
        {
            GameObject pellet = Instantiate(pelletPrefab, transform);
            pellet.SetActive(false);
            pelletPool.Enqueue(pellet);
            yield return null;
        }
    }

    public override void BallShot(BallShotEvent e)
    {
        FireShotgunPellets();
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
                pellet.transform.position = transform.position;
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

    private GameObject GetPelletFromPool()
    {
        if (pelletPool.Count > 0)
        {
            return pelletPool.Dequeue();
        }
        return Instantiate(pelletPrefab, transform);
    }
    
    public void ReturnPelletToPool(GameObject pellet)
    {
        pellet.SetActive(false);
        pelletPool.Enqueue(pellet);
    }

    public override void NextShotCued(NextShotCuedEvent e)
    {
        foreach (GameObject pellet in activePellets)
        {
            ReturnPelletToPool(pellet);
        }
        activePellets.Clear();
    }
}
