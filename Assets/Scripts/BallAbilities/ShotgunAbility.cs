using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunAbility : BallAbility, IBallAbility
{
    public GameObject pelletPrefab;
    public Vector2 spread = new Vector2(1f, .5f);
    private Queue<GameObject> pelletPool = new Queue<GameObject>();
    private List<GameObject> activePellets = new List<GameObject>();
    private const int pelletCount = 50;

    private void Start()
    {
        if (GameStateManager.Instance.CurrentGameState == GameStateManager.GameState.InGame)
        {
            StartCoroutine(InitializePelletPool());
        }
    }

    private void OnEnable()
    {
        GameManager.OnBallShot += BallShot;
        GameManager.OnNextShotCued += NextShotCued;
    }

    private void OnDisable()
    {
        GameManager.OnBallShot -= BallShot;
        GameManager.OnNextShotCued -= NextShotCued;
    }

    private IEnumerator InitializePelletPool()
    {
        for (int i = 0; i < pelletCount; i++)
        {
            GameObject pellet = Instantiate(pelletPrefab);
            pellet.SetActive(false);
            pelletPool.Enqueue(pellet);
            yield return null;
        }
    }

    public void BallShot()
    {
        FireShotgunPellets();
    }

    private void FireShotgunPellets()
    {
        Transform aim = GameManager.Instance.angleInput.cylinderPivot;
        int pelletsToFire = 10;
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
                    rb.linearVelocity = (aim.forward+ spreadOffset).normalized * 80f;
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
        return null;
    }

    public void ReturnPelletToPool(GameObject pellet)
    {
        pellet.SetActive(false);
        pelletPool.Enqueue(pellet);
    }

    public void NextShotCued()
    {
        foreach (GameObject pellet in activePellets)
        {
            ReturnPelletToPool(pellet);
        }
        activePellets.Clear();
    }
}
