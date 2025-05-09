using UnityEngine;

public class BallLandingIndicatorPulse : MonoBehaviour
{
    public Material  material;
    private Material materialInstance;
    private static readonly int StartTimeID = Shader.PropertyToID("_StartTime");

    private void Awake()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            materialInstance = new Material(material);
            renderer.material = materialInstance;
        }
    }

    public void Activate()
    {
        if (materialInstance != null)
        {
            materialInstance.SetFloat(StartTimeID, Time.time);
            gameObject.SetActive(true);
        }
    }
}
