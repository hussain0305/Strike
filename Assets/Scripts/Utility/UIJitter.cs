using UnityEngine;

public class UIJitter : MonoBehaviour
{
    [Header("Jitter Settings")]
    public float positionAmount = 1f;
    public float rotationAmount = 1f;
    public float speed = 10f;

    private RectTransform rectTransform;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private float jitterTime = 2;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
        originalRotation = rectTransform.localRotation;
    }

    void Update()
    {
        float time = Time.time * speed;
        jitterTime -= Time.deltaTime;
        if (jitterTime < 0)
            return;
        
        float offsetX = (Mathf.PerlinNoise(time, 0f) - 0.5f) * 2f * positionAmount;
        float offsetY = (Mathf.PerlinNoise(0f, time) - 0.5f) * 2f * positionAmount;
        float rotationZ = (Mathf.PerlinNoise(time, time) - 0.5f) * 2f * rotationAmount;

        rectTransform.anchoredPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
    }

    void OnDisable()
    {
        rectTransform.anchoredPosition = originalPosition;
        rectTransform.localRotation = originalRotation;
        jitterTime = 2;
    }
}
