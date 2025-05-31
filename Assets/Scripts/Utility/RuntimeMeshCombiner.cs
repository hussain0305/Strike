using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RuntimeMeshCombiner : MonoBehaviour
{
    void Start()
    {
        CombineAllChildren();
    }

    void CombineAllChildren()
    {
        var parentMF = GetComponent<MeshFilter>();
        var parentMR = GetComponent<MeshRenderer>();
        
        var allFilters = GetComponentsInChildren<MeshFilter>();
        int count = allFilters.Length - 1;

        var combine = new CombineInstance[count];
        Material mat = null;
        int i = 0;

        foreach (var meshFilter in allFilters)
        {
            if (meshFilter == parentMF)
                continue;
            
            mat = meshFilter.GetComponent<MeshRenderer>().sharedMaterial;

            combine[i].mesh = meshFilter.sharedMesh;
            combine[i].transform = transform.worldToLocalMatrix * meshFilter.transform.localToWorldMatrix;
            meshFilter.gameObject.SetActive(false);
            i++;
        }

        var combined = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
        combined.CombineMeshes(combine, true, true);
        parentMF.sharedMesh = combined;
        parentMR.sharedMaterial = mat;
    }
}
