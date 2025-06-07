using UnityEditor;
using UnityEngine;

public class GridGeneratorWindow : EditorWindow
{
    private GameObject objectPrefab;
    private Transform parentContainer;
    private Vector3 center = Vector3.zero;
    private float xSpacing = 1f;
    private float ySpacing = 1f;
    private float zSpacing = 1f;
    private int numX = 1;
    private int numY = 1;
    private int numZ = 1;

    [MenuItem("Tools/Grid Generator")]
    public static void ShowWindow()
    {
        GetWindow<GridGeneratorWindow>("Grid Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("3D Grid Generator", EditorStyles.boldLabel);

        objectPrefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Prefab"), objectPrefab, typeof(GameObject), false);

        parentContainer = (Transform)EditorGUILayout.ObjectField(new GUIContent("Parent Container"), parentContainer, typeof(Transform), true);

        GUILayout.Space(5);

        center = EditorGUILayout.Vector3Field("Center Position", center);

        xSpacing = EditorGUILayout.FloatField(new GUIContent("X Spacing"), xSpacing);
        if (xSpacing < 0.01f)
            xSpacing = 0.01f;

        ySpacing = EditorGUILayout.FloatField(new GUIContent("Y Spacing"), ySpacing);
        if (ySpacing < 0.01f)
            ySpacing = 0.01f;

        zSpacing = EditorGUILayout.FloatField(new GUIContent("Z Spacing"), zSpacing);
        if (zSpacing < 0.01f)
            zSpacing = 0.01f;

        GUILayout.Space(5);

        numX = EditorGUILayout.IntField(new GUIContent("Num X"), numX);
        if (numX < 1)
            numX = 1;

        numY = EditorGUILayout.IntField(new GUIContent("Num Y"), numY);
        if (numY < 1)
            numY = 1;

        numZ = EditorGUILayout.IntField(new GUIContent("Num Z"), numZ);
        if (numZ < 1)
            numZ = 1;

        GUILayout.Space(10);

        if (GUILayout.Button("Generate Grid"))
        {
            if (objectPrefab == null)
            {
                Debug.LogError("[GridGenerator] Assign prefab.");
                return;
            }
            GenerateGrid();
        }
    }

    private void GenerateGrid()
    {
        if (parentContainer == null)
        {
            GameObject go = new GameObject($"Grid_{numX}x{numY}x{numZ}");
            parentContainer = go.transform;
        }

        int totalCount = numX * numY * numZ;

        for (int x = 0; x < numX; x++)
        {
            for (int y = 0; y < numY; y++)
            {
                for (int z = 0; z < numZ; z++)
                {
                    float xPos = center.x + (x * xSpacing);
                    float yPos = center.y + (y * ySpacing);
                    float zPos = center.z + (z * zSpacing);
                    Vector3 spawnPos = new Vector3(xPos, yPos, zPos);

                    GameObject spawnedObj = Instantiate(objectPrefab, parentContainer);
                    Undo.RegisterCreatedObjectUndo(spawnedObj, "Spawn Grid Object");
                    spawnedObj.transform.position = spawnPos;
                    spawnedObj.transform.rotation = Quaternion.identity;
                    spawnedObj.transform.SetParent(parentContainer, true);
                }
            }
        }

        Debug.Log($"[GridGenerator] Generated {totalCount} objects in a {numX}×{numY}×{numZ} grid.");
    }
}
