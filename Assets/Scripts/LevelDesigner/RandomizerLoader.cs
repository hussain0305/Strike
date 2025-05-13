using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
        // randomizedHexStack.SpawnHexStacksWithCenter(platformCenter + ySpacing, 2, xSpacing.x, 5, 3,3.5f, collectiblesParent);
        randomizedGutterWall.SpawnOnSides(4, 3, RandomizedMovementOptions.SomeMoving, new IncludeTypesInRandomization(true, true));
    }

}

//Decide what to do
//Objects in the gutter? yes or no?
//how many levels
//
//if yes, moving or not
//

/*
    void SpawnRings(Vector3 center, float R, float d, int ringCount)
    {
        SpawnObject(PoolingManager.Instance.GetObject(PointTokenType.Pin_1x), center, Quaternion.identity);
        float D = 2*R + d;
        float startOffset = Mathf.PI / 6f;  

        for (int ring = 1; ring <= ringCount; ring++)
        {
            float radius = ring * D;
            for (int i = 0; i < 6; i++)
            {
                float angle = startOffset + i * (Mathf.PI / 3f);
                Vector3 offset = new Vector3(
                    radius * Mathf.Cos(angle),
                    0,
                    radius * Mathf.Sin(angle)
                );
                Vector3 hexPos = center + offset;

                Vector3 dirToCenter = (center - hexPos).normalized;
                Quaternion rot = Quaternion.LookRotation(dirToCenter, Vector3.up);

                SpawnObject(PoolingManager.Instance.GetObject(PointTokenType.Pin_1x), hexPos, rot);
            }
        }
    }

    public void SpawnRing(Vector3 center, float R, float d)
    {
        SpawnObject(PoolingManager.Instance.GetObject(PointTokenType.Pin_1x), center, Quaternion.identity);
        float D = 2*R + d;
        float startOffset = Mathf.PI / 6f;
        for(int i = 0; i < 6; i++)
        {
            float angle = startOffset + i * (Mathf.PI / 3f);
            Vector3 offset = new Vector3(
                D * Mathf.Cos(angle),
                0,
                D * Mathf.Sin(angle)
            );
            Vector3 hexPos = center + offset;
            
            Vector3 dirToCenter = (center - hexPos).normalized;
            Quaternion rot = Quaternion.LookRotation(dirToCenter, Vector3.up);
            
            SpawnObject(PoolingManager.Instance.GetObject(PointTokenType.Pin_1x), hexPos, rot);
        }
    }

    public void SpawnObject(GameObject collectibleObject, Vector3 hexPos, Quaternion rot)
    {
        collectibleObject.transform.SetParent(collectiblesParent);
        collectibleObject.transform.position = hexPos;
        collectibleObject.transform.rotation = rot;

        Collectible collectibleScript = collectibleObject.GetComponent<Collectible>();
        if (collectibleScript != null)
        {
            collectibleScript.InitializeAndSetup(GameManager.Context, 5, 1, Collectible.PointDisplayType.InBody);
        }
    }
 */

// bool collectibleMoves = collectibleData.path != null && collectibleData.path.Length > 1;
// ContinuousMovement cmScript = collectibleObject.GetComponent<ContinuousMovement>();
//
// if (collectibleMoves)
// {
//     if (!cmScript)
//         cmScript = collectibleObject.AddComponent<ContinuousMovement>();
//
//     cmScript.pointA = collectibleData.path[0];
//     cmScript.pointB = collectibleData.path[1];
//     cmScript.speed  = collectibleData.movementSpeed;
//         
//     Rigidbody rBody = collectibleObject.GetComponent<Rigidbody>();
//     rBody.isKinematic = true;
// }
// else if (cmScript)
// {
//     Destroy(cmScript);
// }
//     
// DangerToken dangerScript = collectibleObject.GetComponent<DangerToken>();
// if (dangerScript)
// {
//     dangerScript.dangerTokenIndex = currentDangerPinIndex;
//     spawnedDangerObjects.Add(collectibleObject);
//     if (collectibleData.activeOnStart && currentDangerPinIndex > highestActiveDangerPinIndex)
//     {
//         highestActiveDangerPinIndex = currentDangerPinIndex;
//     }
//     currentDangerPinIndex++;
// }
