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
    
    public void SetDoorOpen()
    {
        doorOpen = true;
        door.localScale = new Vector3(door.localScale.x, 0, door.localScale.z);
        door.gameObject.SetActive(false);
    }

    public void SetDoorClosed()
    {
        doorOpen = false;
        door.localScale = new Vector3(door.localScale.x, doorOpenYScale, door.localScale.z);
        door.gameObject.SetActive(true);
    }

    public void Reset()
    {
        SetDoorClosed();
    }
    
    public void SyncForPlayer(bool open)
    {
        if (open)
            SetDoorOpen();
        else
            SetDoorClosed();
    }
}
