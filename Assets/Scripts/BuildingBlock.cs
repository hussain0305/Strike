using UnityEngine;

public class BuildingBlock : MonoBehaviour
{
    public Rigidbody rBody;
    public MeshRenderer edgeMesh;

    public void SetPositiveBlock()
    {
        edgeMesh.sharedMaterial = GlobalAssets.Instance.positiveCollectibleMaterial;
    }

    public void SetNegativeBlock()
    {
        edgeMesh.sharedMaterial = GlobalAssets.Instance.negativeCollectibleMaterial;
    }
}
