using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

public class EditorMeshCombiner : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Combine Selected Children into Mesh Asset")]
    private static void CombineSelectedChildren()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogError("No GameObject selected");
            return;
        }

        var parentGO = Selection.activeGameObject;
        var parentMF = parentGO.GetComponent<MeshFilter>();
        var parentMR = parentGO.GetComponent<MeshRenderer>();

        if (parentMF == null || parentMR == null)
        {
            Debug.LogError("Add a MeshFilter and MeshRenderer on the selected GameObject");
            return;
        }

        var allFilters = parentGO.GetComponentsInChildren<MeshFilter>(true);
        var combineList = new List<CombineInstance>();
        Material sharedMat = null;

        foreach (var meshFilter in allFilters)
        {
            if (meshFilter == parentMF)
                continue;

            var meshRenderer = meshFilter.GetComponent<MeshRenderer>();
            if (meshRenderer == null || meshFilter.sharedMesh == null)
                continue;

            if (sharedMat == null)
                sharedMat = meshRenderer.sharedMaterial;

            CombineInstance ci = new CombineInstance
            {
                mesh = meshFilter.sharedMesh,
                transform = parentGO.transform.worldToLocalMatrix * meshFilter.transform.localToWorldMatrix
            };
            combineList.Add(ci);
        }

        if (combineList.Count == 0)
        {
            Debug.LogWarning($"No child meshes found to combine under {parentGO.name}");
            return;
        }

        var combinedMesh = new Mesh
        {
            name = parentGO.name + "_Combined",
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
        combinedMesh.CombineMeshes(combineList.ToArray(), true, true);

        string parentPath = AssetDatabase.GetAssetPath(parentGO);
        string assetFolder = "Assets/CombinedMeshes";

        string assetPath = AssetDatabase.GenerateUniqueAssetPath(assetFolder + "/" + combinedMesh.name + ".asset");
        AssetDatabase.CreateAsset(combinedMesh, assetPath);
        AssetDatabase.SaveAssets();

        parentMF.sharedMesh = combinedMesh;
        parentMR.sharedMaterial = sharedMat;

        foreach (var meshFilter in allFilters)
        {
            if (meshFilter == parentMF)
                continue;
            
            DestroyImmediate(meshFilter.gameObject);
        }

        EditorUtility.SetDirty(parentGO);
        Debug.Log($"Combined {combineList.Count} child meshes into {assetPath} and assigned to {parentGO.name}");
    }
#endif
}
