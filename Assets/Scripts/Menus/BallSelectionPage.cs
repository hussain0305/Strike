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
    public BallPreviewController previewController;

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
        GameObject selectedBall = SetupBallForPreview(ballIndex);

        BallProperties selectedBallProperties = Balls.Instance.allBalls[ballIndex];
        statWeight.propertyTypeText.text = "WEIGHT";
        statWeight.propertySlider.value =
            Mathf.Clamp01(selectedBallProperties.weight / Balls.Instance.maxWeight);
        statWeight.propertyValueText.text = selectedBallProperties.weight.ToString("F1");
        
        statSpin.propertyTypeText.text = "SPIN";
        statSpin.propertySlider.value =
            Mathf.Clamp01(selectedBallProperties.spin / Balls.Instance.maxSpin);
        statSpin.propertyValueText.text = selectedBallProperties.spin.ToString("F1");
        
        statBounce.propertyTypeText.text = "BOUNCE";
        statBounce.propertySlider.value =
            Mathf.Clamp01(selectedBallProperties.physicsMaterial.bounciness / Balls.Instance.maxBounce);
        statBounce.propertyValueText.text = selectedBallProperties.physicsMaterial.bounciness.ToString("F1");

        ballDescription.text = selectedBallProperties.description;

        RarityAppearance rarityAppearance =
            GlobalAssets.Instance.GetRarityAppearanceSettings(selectedBallProperties.rarity);
        foreach (Image img in frame)
        {
            img.material = rarityAppearance.material;
        }
        rarityText.text = selectedBallProperties.rarity.ToString();
        rarityText.color = rarityAppearance.color;
        
        HighlightSelected(ballIndex);

        previewController.PlayPreview(selectedBallProperties.name, selectedBall);
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

    public GameObject SetupBallForPreview(int ballIndex)
    {
        GameObject selectedBall = null;
        foreach (int bIndex in previewBalls.Keys)
        {
            previewBalls[bIndex].SetActive(false);
            if (bIndex == ballIndex)
            {
                selectedBall = previewBalls[bIndex];
                selectedBall.SetActive(true);
            }
        }

        if (!selectedBall)
        {
            selectedBall = Instantiate(Balls.Instance.allBalls[ballIndex].prefab, previewBallsParent);
            selectedBall.transform.localScale *= 0.25f;
            previewBalls.Add(ballIndex, selectedBall);
        }

        selectedBall.transform.position = previewController.tee.ballPosition.position;
        MainMenu.Context.InitPreview(selectedBall.GetComponent<Ball>(),
            previewController.aimTransform,
            previewController.trajectory,
            previewController.tee);
        return selectedBall;
    }
}
