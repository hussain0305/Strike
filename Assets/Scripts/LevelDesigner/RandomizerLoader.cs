using UnityEngine;

[System.Serializable]
public struct EdgeDefinition
{
    public Transform end1;
    public Transform end2;
}
public class RandomizerLoader : LevelLoader
{
    [Header("Randomized Level")]
    public Transform[] hexGutterVertices;
    public EdgeDefinition[] hexGutterCupSides;
    public Transform[] hexVertices;
    
    public int maxObjects = 50;

    [HideInInspector]
    public Vector3 platformCenter = new Vector3(0, 0, 50);
    private Vector3 xSpacing = new Vector3(0.2f, 0, 0);
    private Vector3 ySpacing = new Vector3(0, 0.2f, 0);
    private Vector3 zSpacing = new Vector3(0, 0, 0.2f);

    private RandomizedHexStack randomizedHexStack;
    private RandomizedGutterWall randomizedGutterWall;

    private void Awake()
    {
        randomizedHexStack = gameObject.AddComponent<RandomizedHexStack>();
        randomizedHexStack.Initialize(this);
        
        randomizedGutterWall = gameObject.AddComponent<RandomizedGutterWall>();
        randomizedGutterWall.Initialize(this);

    }

    public override int GetTargetPoints()
    {
        return 10;
    }

    override public void LoadLevel()
    {
        //Make Decisions here
        bool spawnHexStack = true;
        if (spawnHexStack)
        {
            SpawnHexStack(Vector3.zero);
        }
        randomizedGutterWall.SpawnOnSides(4, 3, RandomizedMovementOptions.SomeMoving, new IncludeTypesInRandomization(true, true));
    }

    public void SpawnHexStack(Vector3 offsetFromCenter)
    {
        PointTokenType[] allHexTokens = new PointTokenType[] { PointTokenType.Pin_1x , PointTokenType.Pin_2x, PointTokenType.Pin_4x};
        PointTokenType randomHexToken = allHexTokens[Random.Range(0, allHexTokens.Length)];
        Vector3 dimensions = PoolingManager.Instance.GetPointTokenDimension(randomHexToken);

        int numRings = 2;
        int numLevels = 3;

        switch (randomHexToken)
        {
            case PointTokenType.Pin_1x:
                numRings = Random.Range(3,5);
                numLevels = Mathf.Min(Random.Range(3,5), numRings);
                break;
            case PointTokenType.Pin_2x:
                numRings = Random.Range(2,4);
                numLevels = Mathf.Min(Random.Range(2,4), numRings);
                break;
            case PointTokenType.Pin_4x:
                numRings = 2;
                numLevels = Mathf.Min(Random.Range(1,2), numRings);
                break;
        }
        
        randomizedHexStack.SpawnHexStacksWithCenter(randomHexToken, platformCenter + ySpacing, dimensions.x / 2, dimensions.y, 
            xSpacing.x, numRings, numLevels, collectiblesParent);
    }
}