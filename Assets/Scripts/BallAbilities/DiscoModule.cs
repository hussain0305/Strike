using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoModule : IBallAbilityUpdateableModule
{
    Ball ball;
    IContextProvider context;
    
    private Queue<GameObject> discoPelletsPool = new Queue<GameObject>();
    private List<GameObject> activeDiscoPellets = new List<GameObject>();
    private int pelletCount;
    private int pelletsToFire;
    private Vector2 spread;
    private GameObject[] discoPelletPrefabs;
    private IBallAbilityUpdateableModule ballAbilityUpdateableModuleImplementation;

    public AbilityAxis Axis => AbilityAxis.Spawn;

    private float lastY = Mathf.NegativeInfinity;
    private bool pelletsSpawned = false;
    private Coroutine pelletsSpawnRoutine;

    public DiscoModule(GameObject[] _discoPelletPrefabs, Vector2 _spread, int _poolSize, int _pelletsToFire)
    {
        discoPelletPrefabs = _discoPelletPrefabs;
        spread = _spread;
        pelletCount = _poolSize;
        pelletsToFire = _pelletsToFire;
    }
    
    public void Initialize(Ball ownerBall, IContextProvider _context)
    {
        ball = ownerBall;
        context  = _context;

        CoroutineDispatcher.Instance.RunCoroutine(InitializePelletPool());
        // ball.StartCoroutine(InitializePelletPool());
    }
    
    public void OnUpdate()
    {
        // if (GameManager.BallState == BallState.OnTee || pelletsSpawned)
        //     return;
        if (context.GetBallState() == BallState.OnTee || pelletsSpawned)
            return;

        if (ball.transform.position.y >= lastY)
        {
            lastY = ball.transform.position.y;
            return;
        }

        pelletsSpawned = true;
        if (pelletsSpawnRoutine != null)
        {
            ball.StopCoroutine(pelletsSpawnRoutine);
        }
        pelletsSpawnRoutine = ball.StartCoroutine(DoDiscoRoutine());
    }


    public void OnProjectilesSpawned(ProjectilesSpawnedEvent e) { }
    public void OnHitSomething(BallHitSomethingEvent e) { }

    public void OnBallShot(BallShotEvent e)
    {
        lastY = Mathf.NegativeInfinity;
    }

    public void OnNextShotCued(NextShotCuedEvent e)
    {
        foreach (GameObject pellet in activeDiscoPellets)
        {
            ReturnPelletToPool(pellet);
        }
        activeDiscoPellets.Clear();
        
        pelletsSpawned = false;
        if (pelletsSpawnRoutine != null)
        {
            ball.StopCoroutine(pelletsSpawnRoutine);
            pelletsSpawnRoutine = null;
        }
        ball.haltBall = false;
        lastY = Mathf.NegativeInfinity;
    }
    
    private IEnumerator InitializePelletPool()
    {
        discoPelletsPool = new Queue<GameObject>();
        for (int i = 0; i < pelletCount; i++)
        {
            GameObject pellet = Object.Instantiate(discoPelletPrefabs[Random.Range(0, discoPelletPrefabs.Length)]);
            pellet.transform.localScale = ball.transform.localScale / 5;
            pellet.SetActive(false);
            discoPelletsPool.Enqueue(pellet);
            yield return null;
        }
        EventBus.Publish(new ProjectilesSpawnedEvent(discoPelletsPool.ToArray()));
    }

    private GameObject GetPelletFromPool()
    {
        if (discoPelletsPool.Count > 0)
        {
            return discoPelletsPool.Dequeue();
        }
        return Object.Instantiate(discoPelletPrefabs[Random.Range(0, discoPelletPrefabs.Length)], ball.transform);
    }

    public void ReturnPelletToPool(GameObject pellet)
    {
        pellet.SetActive(false);
        discoPelletsPool.Enqueue(pellet);
    }

    public IEnumerator DoDiscoRoutine()
    {
        ball.haltBall = true;
        float totalDiscoCoroutineTime = 1.5f;
        float intervalBetweenSpawns = (totalDiscoCoroutineTime - 0.1f) / pelletsToFire;
        WaitForSeconds wait = new WaitForSeconds(intervalBetweenSpawns);
        
        int firedCount = 0;
        float timePassed = 0;
        float timeTillNextSpawn = intervalBetweenSpawns;
        while (timePassed < totalDiscoCoroutineTime &&  firedCount < pelletsToFire)
        {
            if (timeTillNextSpawn >= intervalBetweenSpawns)
            {
                timeTillNextSpawn = 0;
                FireDiscoPellet();
            }
            timeTillNextSpawn += Time.deltaTime;
            timePassed += Time.deltaTime;
            
            ball.transform.Rotate(Vector3.up, 720 * Time.deltaTime, Space.Self);
            
            yield return null;
        }

        ball.haltBall = false;
        pelletsSpawnRoutine = null;
    }
    
    private void FireDiscoPellet()
    {
        GameObject pellet = GetPelletFromPool();
        if (pellet != null)
        {
            pellet.transform.position = ball.transform.position;
            pellet.SetActive(true);
            activeDiscoPellets.Add(pellet);
            Rigidbody rb = pellet.GetComponent<Rigidbody>();

            float horizontalAngle = Random.Range(0f, Mathf.PI * 2f);
            float horizontalX = Mathf.Cos(horizontalAngle);
            float horizontalZ = Mathf.Sin(horizontalAngle);
            float xVel = horizontalX * spread.x;
            float zVel = horizontalZ * spread.x;
            float yVel = Random.Range(-spread.y, spread.y);
            rb.linearVelocity = new Vector3(xVel, yVel, zVel);
        }
    }

    public void Cleanup()
    {
        GameObject[] pelletArray = discoPelletsPool.ToArray();
        for (int i = 0; i < pelletArray.Length; i++)
        {
            GameObject.Destroy(pelletArray[i]);   
        }
    }
}
