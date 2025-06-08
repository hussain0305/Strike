using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using static UnityEditor.PrefabUtility;

public class HoneycombPointTopGeneratorWindow : EditorWindow
{
    private GameObject hexPrefab;
    private Vector3 center = Vector3.zero;
    private float hexSize = 1f;
    private int rings = 1;
    private Transform parentContainer;

    [MenuItem("Tools/Honeycomb Generator Point-Top")]
    public static void ShowWindow()
    {
        GetWindow<HoneycombPointTopGeneratorWindow>("Honeycomb Generator Point-Top");
    }

    private void OnGUI()
    {
        GUILayout.Label("Honeycomb Point-Top Layout Generator", EditorStyles.boldLabel);

        hexPrefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Hex Prefab"), hexPrefab, typeof(GameObject), true);

        center = EditorGUILayout.Vector3Field("Center Position", center);

        hexSize = EditorGUILayout.FloatField(new GUIContent("Hex Size"), hexSize);
        if (hexSize < 0.01f)
            hexSize = 0.01f;

        rings = EditorGUILayout.IntField(new GUIContent("Rings"), rings);
        if (rings < 1)
            rings = 1;

        parentContainer = (Transform)EditorGUILayout.ObjectField(new GUIContent("Parent Container"), parentContainer, typeof(Transform), true);

        GUILayout.Space(10);

        if (GUILayout.Button("Generate Honeycomb"))
        {
            if (hexPrefab == null)
            {
                Debug.LogError("Assign Prefab");
                return;
            }
            GenerateHoneycomb();
        }
    }

    private void GenerateHoneycomb()
    {
        int radius = rings - 1;

        if (parentContainer == null)
        {
            GameObject go = new GameObject($"Honeycomb_{rings}Rings");
            parentContainer = go.transform;
        }

        float width = 2f * hexSize;
        float height = Mathf.Sqrt(3f) * hexSize;
        
        List<Vector2Int> axialCoords = new List<Vector2Int>();
        for (int q = -radius; q <= radius; q++)
        {
            int rMin = Mathf.Max(-radius, -q - radius);
            int rMax = Mathf.Min(radius, -q + radius);
            for (int r = rMin; r <= rMax; r++)
            {
                axialCoords.Add(new Vector2Int(q, r));
            }
        }

        foreach (var hex in axialCoords)
        {
            int q = hex.x;
            int r = hex.y;

            float xPos = hexSize * 1.5f * q;
            float yPos = hexSize * Mathf.Sqrt(3f) * (r + q * 0.5f);
            Vector3 spawnPos = new Vector3(center.x + xPos, center.y + yPos, center.z);

            GameObject spawnedHex = Instantiate(hexPrefab, parentContainer);
            Undo.RegisterCreatedObjectUndo(spawnedHex, "Spawn Hex");
            spawnedHex.transform.position = spawnPos;
            spawnedHex.transform.rotation = Quaternion.identity;
            spawnedHex.transform.SetParent(parentContainer, true);
        }

        Debug.Log($"[HoneycombGenerator] Generated {axialCoords.Count} hexes with {rings} ring(s).");
    }
}
