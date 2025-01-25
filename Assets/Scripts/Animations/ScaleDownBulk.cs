using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleDownBulk : MonoBehaviour
{
    public float scalingTime;
    public ScalingData[] scalingData;
    public ScaleUpBulk pairedScaleUp;
    
    public void StartScalingDown()
    {
        pairedScaleUp?.CancelScalingUp();
        StartCoroutine(ScalingCoroutine());
    }

    public IEnumerator ScalingCoroutine()
    {
        foreach (ScalingData scaleData in scalingData)
        {
            foreach (Transform currentObject in scaleData.objectsToScale)
            {
                currentObject.gameObject.SetActive(true);
            }
        }

        float timePassed = 0;
        while (timePassed <= scalingTime)
        {
            float lerpValue = Mathf.Clamp01(timePassed / scalingTime);
            
            foreach (ScalingData scaleData in scalingData)
            {
                foreach (Transform currentObject in scaleData.objectsToScale)
                {
                    currentObject.localScale = Vector3.Lerp(scaleData.scaleInLevel, Vector3.zero, Easings.EaseInBack(lerpValue));
                }
            }
            
            timePassed += Time.deltaTime;
            yield return null;
        }
        
        foreach (ScalingData scaleData in scalingData)
        {
            foreach (Transform currentObject in scaleData.objectsToScale)
            {
                currentObject.gameObject.SetActive(false);
            }
        }
    }

    public void CancelScalingDown()
    {
        StopAllCoroutines();
    }
}
