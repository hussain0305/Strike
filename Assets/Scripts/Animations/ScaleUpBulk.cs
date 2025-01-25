using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleUpBulk : MonoBehaviour
{
    public float scalingTime;
    public ScalingData[] scalingData;
    public ScaleDownBulk pairedScaleDown;
    
    public void StartScalingUp()
    {
        pairedScaleDown?.CancelScalingDown();
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
                    currentObject.localScale = scaleData.scaleInLevel * Easings.EaseOutBack(lerpValue);
                    // currentObject.localScale = Vector3.Lerp(Vector3.zero, scaleData.scaleInLevel, Easings.EaseOutBack(lerpValue));
                }
            }
            
            timePassed += Time.deltaTime;
            yield return null;
        }
        
        foreach (ScalingData scaleData in scalingData)
        {
            foreach (Transform currentObject in scaleData.objectsToScale)
            {
                currentObject.localScale = scaleData.scaleInLevel;
            }
        }
    }

    public void CancelScalingUp()
    {
        StopAllCoroutines();
    }
    
}
