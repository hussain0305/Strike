using System;
using System.Collections.Generic;
using UnityEngine;

public class BallSelectionPage : MonoBehaviour
{
    public Transform previewBallsParent;
    public BallSelectionButton ballSelectionPrefab;
    public Transform ballSelectionButtonsParent;
    public BallStatRow statWeight;
    public BallStatRow statSpin;
    public BallStatRow statBounce;
    
    private Dictionary<int, GameObject> previewBalls;
    private Dictionary<int, BallSelectionButton> ballButtons;
    
    private static BallSelectionPage instance;
    public static BallSelectionPage Instance => instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public void Start()
    {
        SaveManager.RegisterListener(this);
        SaveManager.OnSaveFileLoaded += SaveLoaded;
    }
    
    public void SaveLoaded()
    {
        previewBalls = new Dictionary<int, GameObject>();
        ballButtons = new Dictionary<int, BallSelectionButton>();
        SpawnButtonsAndSetSelected();
        SaveManager.OnSaveFileLoaded -= SaveLoaded;
        SaveManager.MarkListenerComplete(this);
        gameObject.SetActive(false);
    }

    public void SpawnButtonsAndSetSelected()
    {
        int i = 0;
        foreach (BallProperties ballProperty in Balls.Instance.allBalls)
        {
            BallSelectionButton spawnedSelectionButton = Instantiate(ballSelectionPrefab, ballSelectionButtonsParent);
            spawnedSelectionButton.SetBallName(ballProperty.name);
            spawnedSelectionButton.SetBallIndex(i);
            ballButtons.Add(i, spawnedSelectionButton);
            i++;
        }

        int currentSelectedBall = SaveManager.GetSelectedBall();
        if (ballButtons.ContainsKey(currentSelectedBall))
        {
            ballButtons[currentSelectedBall].SetSelected();
            SetSelectedBall(currentSelectedBall);
        }
    }
    
    public void SetSelectedBall(int ballIndex)
    {
        ActivateBallPreview(ballIndex);

        statWeight.propertyTypeText.text = "WEIGHT";
        statWeight.propertySlider.value =
            Mathf.Clamp01(Balls.Instance.allBalls[ballIndex].weight / Balls.Instance.maxWeight);
        statWeight.propertyValueText.text = Balls.Instance.allBalls[ballIndex].weight.ToString("F2");
        
        statSpin.propertyTypeText.text = "SPIN";
        statSpin.propertySlider.value =
            Mathf.Clamp01(Balls.Instance.allBalls[ballIndex].spin / Balls.Instance.maxSpin);
        statSpin.propertyValueText.text = Balls.Instance.allBalls[ballIndex].spin.ToString("F2");
        
        statBounce.propertyTypeText.text = "BOUNCE";
        statBounce.propertySlider.value =
            Mathf.Clamp01(Balls.Instance.allBalls[ballIndex].bounce / Balls.Instance.maxBounce);
        statBounce.propertyValueText.text = Balls.Instance.allBalls[ballIndex].bounce.ToString("F2");
    }
    
    public void ActivateBallPreview(int ballIndex)
    {
        bool existingBallFound = false;
        foreach (int bIndex in previewBalls.Keys)
        {
            previewBalls[bIndex].SetActive(false);
            if (bIndex == ballIndex)
            {
                previewBalls[bIndex].SetActive(true);
                existingBallFound = true;
            }
        }

        if (!existingBallFound)
        {
            GameObject spawnedBall = Instantiate(Balls.Instance.allBalls[ballIndex].prefab, previewBallsParent);
            previewBalls.Add(ballIndex, spawnedBall);
        }
    }
}
