using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BallSelectionPage : MonoBehaviour
{
    public Transform previewBallsParent;
    public BallSelectionButton ballSelectionPrefab;
    public Transform ballSelectionButtonsParent;
    public BallStatRow statWeight;
    public BallStatRow statSpin;
    public BallStatRow statBounce;
    public Button equipBallButton;
    public TextMeshProUGUI ballDescription;

    [Header("Rarity")]
    public Image[] frame;
    public TextMeshProUGUI rarityText;
    
    private Dictionary<int, GameObject> previewBalls;
    private Dictionary<int, BallSelectionButton> ballButtons;

    private int currentSelectedBall;
    private bool setupComplete = false;
    
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
        SaveManager.OnSaveFileLoaded += SetupBallSelection;
    }

    private void OnEnable()
    {
        equipBallButton.onClick.AddListener(() =>
        {
            SaveManager.SetSelectedBall(currentSelectedBall);
        });

        if (SaveManager.IsSaveLoaded && setupComplete)
        {
            SetSelectedBall(SaveManager.GetSelectedBall());
        }
    }

    private void OnDisable()
    {
        equipBallButton.onClick.RemoveAllListeners();
    }

    public void SetupBallSelection()
    {
        previewBalls = new Dictionary<int, GameObject>();
        ballButtons = new Dictionary<int, BallSelectionButton>();
        SpawnButtonsAndSetSelected();
        SaveManager.OnSaveFileLoaded -= SetupBallSelection;
        SaveManager.MarkListenerComplete(this);
        gameObject.SetActive(false);
        setupComplete = true;
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

        SetSelectedBall(SaveManager.GetSelectedBall());
    }
    
    public void SetSelectedBall(int ballIndex)
    {
        currentSelectedBall = ballIndex;
        ActivateBallPreview(ballIndex);

        statWeight.propertyTypeText.text = "WEIGHT";
        statWeight.propertySlider.value =
            Mathf.Clamp01(Balls.Instance.allBalls[ballIndex].weight / Balls.Instance.maxWeight);
        statWeight.propertyValueText.text = Balls.Instance.allBalls[ballIndex].weight.ToString("F1");
        
        statSpin.propertyTypeText.text = "SPIN";
        statSpin.propertySlider.value =
            Mathf.Clamp01(Balls.Instance.allBalls[ballIndex].spin / Balls.Instance.maxSpin);
        statSpin.propertyValueText.text = Balls.Instance.allBalls[ballIndex].spin.ToString("F1");
        
        statBounce.propertyTypeText.text = "BOUNCE";
        statBounce.propertySlider.value =
            Mathf.Clamp01(Balls.Instance.allBalls[ballIndex].physicsMaterial.bounciness / Balls.Instance.maxBounce);
        statBounce.propertyValueText.text = Balls.Instance.allBalls[ballIndex].physicsMaterial.bounciness.ToString("F1");

        ballDescription.text = Balls.Instance.allBalls[ballIndex].description;

        RarityAppearance rarityAppearance =
            GlobalAssets.Instance.GetRarityAppearanceSettings(Balls.Instance.allBalls[ballIndex].rarity);
        foreach (Image img in frame)
        {
            img.material = rarityAppearance.material;
        }
        rarityText.text = Balls.Instance.allBalls[ballIndex].rarity.ToString();
        rarityText.color = rarityAppearance.color;
        
        HighlightSelected(ballIndex);
    }
    
    public void HighlightSelected(int ballIndex)
    {
        foreach (int buttonIndex in ballButtons.Keys)
        {
            if (buttonIndex == ballIndex)
            {
                ballButtons[buttonIndex].SetSelected();
            }
            else
            {
                ballButtons[buttonIndex].SetUnselected();
            }
        }
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
