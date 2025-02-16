using System;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private static LevelManager instance;
    public static LevelManager Instance => instance;

    public Transform collectiblesWorldParent;
    public Transform collectiblesWorldCanvasParent;
    public Transform starsParent;
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(instance.gameObject);
        }
        instance = this;
    }
}
