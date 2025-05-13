using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AbyssTogglerVisuals : MonoBehaviour
{
    public Color OnColor;
    public Color OffColor;
    public Image toggleImage;
    public Transform onPosition;
    public Transform offPosition;
    public GameObject onSection;
    public GameObject offSection;
    
    private bool val = false;
    public bool Value => val;
    private Coroutine toggleCoroutine;
    
    private void Start()
    {
        SetupVisual();
    }

    private void OnEnable()
    {
        if (toggleCoroutine != null)
        {
            StopCoroutine(toggleCoroutine);
        }
        toggleCoroutine = null;
    }

    public void Toggle()
    {
        if (toggleCoroutine != null)
        {
            return;
        }
        
        toggleCoroutine = StartCoroutine(ToggleCoroutine());
    }

    public void SetValue(bool newValue)
    {
        if (val == newValue)
        {
            return;
        }
        Toggle();
    }
    
    public void SetupVisual()
    {
        toggleImage.gameObject.SetActive(false);
        onSection.SetActive(val);
        offSection.SetActive(!val);
    }
    
    IEnumerator ToggleCoroutine()
    {
        Transform toggleImageStart = val ? onPosition.transform : offPosition.transform;
        Transform toggleImageEnd = val ? offSection.transform : onSection.transform;
        Color startColor = val ? OnColor : OffColor;
        Color endColor = val ? OffColor : OnColor;
        onSection.SetActive(false);
        offSection.SetActive(false);
        toggleImage.gameObject.SetActive(true);

        float timePassed = 0;
        float timeToAnimate = 0.25f;
        while (timePassed <= timeToAnimate)
        {
            float lerpVal = timePassed / timeToAnimate;
            toggleImage.transform.position = Vector3.Lerp(toggleImageStart.position, toggleImageEnd.position, lerpVal);
            toggleImage.color = Color.Lerp(startColor, endColor, lerpVal);
            timePassed += Time.deltaTime;
            yield return null;
        }
        val = !val;
        SetupVisual();
        toggleCoroutine = null;
    }
}
