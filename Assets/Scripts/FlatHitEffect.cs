using UnityEngine;

public class FlatHitEffect : MonoBehaviour
{
    private Material materialInstance;
    private static readonly int StartTimeID = Shader.PropertyToID("_StartTime");

    private void Awake()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            materialInstance = new Material(GlobalAssets.Instance.flatHitEffectMaterial);
            renderer.material = materialInstance;
        }
    }

    public void ActivateHitEffect()
    {
        if (materialInstance != null)
        {
            materialInstance.SetFloat(StartTimeID, Time.time);
            gameObject.SetActive(true);
        }
    }
}