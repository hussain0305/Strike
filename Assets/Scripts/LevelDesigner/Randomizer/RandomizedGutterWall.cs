using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum RandomizedMovementOptions
{
    NoMovement,
    SomeMoving,
    AllMoving
}

public struct IncludeTypesInRandomization
{
    public bool pointTokens;
    public bool dangerPins;

    public IncludeTypesInRandomization(bool pointTokens, bool dangerPins)
    {
        this.pointTokens = pointTokens;
        this.dangerPins = dangerPins;
    }
}

public class RandomizedGutterWall : RandomizerSpawner
{
    private float hexSideLength = -1f;
    private float HexSideLength
    {
        get
        {
            if (hexSideLength < 0f)
            {
                EdgeDefinition edge = randomizer.hexGutterCupSides[0];
                var e0 = edge.end1.position;
                var e1 = edge.end2.position;
                hexSideLength = Vector3.Distance(e0, e1);
            }
            return hexSideLength;
        }
    }

    public void SpawnOnSides(int numSides, int numLevels, RandomizedMovementOptions movementOptions, IncludeTypesInRandomization types)
    {
        StartCoroutine(SpawnCoroutine(numSides, numLevels, movementOptions, types));
    }

    private IEnumerator SpawnCoroutine(int numSides, int numLevels, RandomizedMovementOptions movementOptions, IncludeTypesInRandomization types)
    {
        var objDimension = CollectiblePrefabMapping.Instance.GetPointTokenDimension(PointTokenType.Cuboid_Gutter);
        
        float tokenHeight = objDimension.y;
        float tokenLength = objDimension.x;
        float halftokenLength = tokenLength / 2;

        var sides = new List<int> {0,1,2,3};
        Shuffle(sides);
        sides = sides.GetRange(0, Mathf.Min(numSides, sides.Count));

        foreach (var side in sides)
        {
            var end1 = randomizer.hexGutterCupSides[side].end1.position;
            var end2 = randomizer.hexGutterCupSides[side].end2.position;
            var End1to2 = end2 - end1;
            var End2to1 = end1 - end2;
            end1 += (0.05f * End1to2);
            end2 += (0.05f * End2to1);
            
            var dir = (end2 - end1).normalized;
            float edgeLength = Vector3.Distance(end1, end2);
            int maxObjects = Mathf.FloorToInt(edgeLength / tokenLength);

            for (int lvl = 0; lvl < numLevels; lvl++)
            {
                int count = Random.Range(1, maxObjects + 1);

                if (count < 1)
                    continue;

                float yOff = lvl * (tokenHeight + 0.2f);
                Vector3 end1Level = new Vector3(end1.x, yOff, end1.z);
                Vector3 end2Level = new Vector3(end2.x, yOff, end2.z);
                Vector3 centerPointOfEdge = (end1 + end2) * 0.5f;
                Vector3 centerPointOfLevel = (end1Level + end2Level) * 0.5f;
                float spacing = (count == 1)
                    ? 0f
                    : (edgeLength - (count * tokenLength)) / (count + 1);

                bool movesAll = movementOptions == RandomizedMovementOptions.AllMoving;
                bool movesSome = movementOptions == RandomizedMovementOptions.SomeMoving;
                
                Stack<Vector3[]> movementEndPoints = new Stack<Vector3[]>();
                Stack<Vector3> spawnPoints = new Stack<Vector3>();

                if (count == 1)
                {
                    movementEndPoints.Push(new Vector3[] { end1Level + (dir * halftokenLength), end2Level - (dir * halftokenLength) });
                    float dist = edgeLength / 2f;
                    var pos = end1Level + dir * dist;
                    spawnPoints.Push(pos);
                }
                else if (count == 2)
                {
                    movementEndPoints.Push(new Vector3[] { end1Level + (dir * halftokenLength), centerPointOfLevel - (dir * halftokenLength) });
                    movementEndPoints.Push(new Vector3[] { centerPointOfLevel + (dir * halftokenLength), end2Level - (dir * halftokenLength) });
                    float dist = spacing + halftokenLength;
                    var pos = end1Level + dir * dist;
                    spawnPoints.Push(pos);
                    dist = spacing + tokenLength + spacing + halftokenLength;
                    pos = end1Level + dir * dist;
                    spawnPoints.Push(pos);
                }

                for (int i = 0; i < count; i++)
                {
                    var pos = spawnPoints.Pop();
                    Vector3[] endPoints = movementEndPoints.Pop();
                    Vector3 outward = (centerPointOfEdge - randomizer.platformCenter).normalized;  
                    Quaternion rotEdge = Quaternion.LookRotation(outward, Vector3.up);

                    bool spawnPoint = types.pointTokens && (!types.dangerPins || Random.value < 0.76f);
                    bool shouldMove = movesAll || (movesSome && Random.value < 0.5f) && count <= 2;

                    SpawnObject(spawnPoint, pos, rotEdge, randomizer.collectiblesParent, shouldMove, endPoints);
                }

                yield return null;
            }
        }
    }

    private void SpawnObject(bool spawnPoint, Vector3 pos, Quaternion rot, Transform parent, bool shouldMove, Vector3[] path)
    {
        GameObject obj = spawnPoint
            ? PoolingManager.Instance.GetObject(PointTokenType.Cuboid_Gutter)
            : PoolingManager.Instance.GetObject(DangerTokenType.Cube_3x3);
        var rb = obj.GetComponent<Rigidbody>();
        if (rb)
            rb.isKinematic = true;
                    
        obj.transform.SetParent(parent, false);
        obj.transform.position = pos;
        obj.transform.rotation = rot;

        if (spawnPoint)
        {
            PointToken pointToken = obj.GetComponent<PointToken>();
            pointToken.InitializeAndSetup(GameManager.Context, 2, 1, Collectible.PointDisplayType.InBody);
        }
        
        var cm = obj.GetComponent<ContinuousMovement>();
        if (shouldMove)
        {
            if (cm == null)
                cm = obj.AddComponent<ContinuousMovement>();
                        
            cm.pointA = path[0];
            cm.pointB = path[1];
            cm.speed = 1f;
        }
        else if (cm != null)
        {
            Destroy(cm);
        }
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            T tmp = list[i]; list[i] = list[r]; list[r] = tmp;
        }
    }
}
