using System;
using System.Collections;
using UnityEngine;

public class SwitchDoor : Obstacle, ISwitchable
{
    public Transform door;

    private float doorOpenTime = 0.5f;
    private bool doorOpen = false;
    private float doorOpenYScale;

    private void Awake()
    {
        doorOpenYScale = door.localScale.y;
    }

    public void Switched(bool switchedOn)
    {
        doorOpen = switchedOn;
        StopAllCoroutines();
        StartCoroutine(DoorStateChange());
    }

    private IEnumerator DoorStateChange()
    {
        door.gameObject.SetActive(true);
        float timePassed = 0;

        float yStart = doorOpen ? doorOpenYScale : 0;
        float yEnd = doorOpen ? 0 : doorOpenYScale;
        float x = door.localScale.x;
        float z = door.localScale.z;
        
        while (timePassed < doorOpenTime)
        {
            door.localScale = new Vector3(x, Mathf.Lerp(yStart, yEnd, timePassed / doorOpenTime), z);
            
            timePassed += Time.deltaTime;
            yield return null;
        }

        door.gameObject.SetActive(!doorOpen);
    }
}
