using UnityEngine;

public class CombineChildCubes : MonoBehaviour
{
    void Start()
    {
        // Get all child cube filters
        var filters = GetComponentsInChildren<MeshFilter>();
        var combine = new CombineInstance[filters.Length];
        
        for (int i = 0; i < filters.Length; i++)
        {
            combine[i].mesh      = filters[i].sharedMesh;
            combine[i].transform = filters[i].transform.localToWorldMatrix;
            filters[i].gameObject.SetActive(false);
        }

        // Create & assign the combined mesh
        var mf = GetComponent<MeshFilter>();
        mf.mesh = new Mesh();
        mf.mesh.CombineMeshes(combine, true, true);

        // Re-enable this object’s renderer
        GetComponent<MeshRenderer>().enabled = true;
    }
}