using System;
using UnityEngine;

public class DisableBetweenShots : MonoBehaviour
{
    public GameObject[] subjects;

    private void OnEnable()
    {
        EventBus.Subscribe<BallShotEvent>(BallShot);
        EventBus.Subscribe<NextShotCuedEvent>(NextShotCued);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<BallShotEvent>(BallShot);
        EventBus.Unsubscribe<NextShotCuedEvent>(NextShotCued);
    }

    public void BallShot(BallShotEvent e)
    {
        foreach (GameObject subject in subjects)
        {
            subject.SetActive(false);
        }
    }

    public void NextShotCued(NextShotCuedEvent e)
    {
        foreach (GameObject subject in subjects)
        {
            subject.SetActive(true);
        }
    }
}
