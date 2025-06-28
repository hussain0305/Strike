using System;
using UnityEngine;

public class BuildingBlock : MonoBehaviour
{
    public Rigidbody rBody;
    public MeshRenderer edgeMesh;

    private void Awake()
    {
        rBody.solverIterations = 1;
    }

    public void SetPositiveBlock()
    {
        edgeMesh.sharedMaterial = GlobalAssets.Instance.positiveCollectibleMaterial;
    }

    public void SetNegativeBlock()
    {
        edgeMesh.sharedMaterial = GlobalAssets.Instance.negativeCollectibleMaterial;
    }
}
