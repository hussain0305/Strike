using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PressAnyKeyToStart : MonoBehaviour
{
    public GameObject notReadySection;
    public GameObject readySection;
    
    private bool isKeyPressed = false;

    private void OnEnable()
    {
        SaveManager.OnSaveFileLoaded += SaveLoaded;
        SaveManager.LoadData();
    }

    private void OnDisable()
    {
        SaveManager.OnSaveFileLoaded -= SaveLoaded;
    }

    public void SaveLoaded()
    {
        notReadySection.SetActive(false);
        readySection.SetActive(true);
        ModeSelector.Instance.Init();
    }
    
    void Update()
    {
        if(SaveManager.IsSaveLoaded && !isKeyPressed && (Input.anyKey || Input.GetMouseButtonDown(0)))
        {
            isKeyPressed = true;
            gameObject.SetActive(false);
        }
    }
}