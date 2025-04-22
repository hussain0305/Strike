using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

[System.Serializable]
public struct FusionBallProperties
{
    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI abilityAxisText;

    public void Set(BallProperties properties, string suffix)
    {
        nameLabel.text = $"{properties.name.ToUpper()}";// [{suffix}]
        abilityAxisText.text = properties.abilityAxis.ToString();
    }

    public void Reset()
    {
        nameLabel.text = "PRIMARY";
        abilityAxisText.text = "";
    }
}

[System.Serializable]
public struct FusedBallProperties
{
    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI primaryInheritedProperty;
    public TextMeshProUGUI secondaryInheritedProperty;

    public void Set(BallProperties primary, BallProperties secondary)
    {
        nameLabel.text = $"{primary.name.ToUpper()}-{secondary.name.ToUpper()}";// [FUSED]
        primaryInheritedProperty.text = primary.abilityText;
        secondaryInheritedProperty.text = secondary.abilityText;
    }

    public void Reset()
    {
        nameLabel.text = "FUSED";
        primaryInheritedProperty.text = "";
        secondaryInheritedProperty.text = "";
    }
}

public class BallFusionController : MonoBehaviour
{
    [Header("Slots & Popups")]
    [SerializeField] private Button primarySlotButton;
    [SerializeField] private Button secondarySlotButton;
    [SerializeField] private GameObject fusionPopup;
    [SerializeField] private Transform popupContentParent;
    [SerializeField] private FusionBallButton fusionBallButtonPrefab;

    [Header("Stat Panel")]
    [SerializeField] private GameObject primaryStatPanel;
    [SerializeField] private GameObject secondaryStatPanel;
    [SerializeField] private GameObject fusedStatPanel;
    [SerializeField] private GameObject addToFavoritesButton;
    [SerializeField] private GameObject fuseButton;
    [SerializeField] private GameObject equipButton;
    [SerializeField] private FusionBallProperties primaryBallStats;
    [SerializeField] private FusionBallProperties secondaryBallStats;
    [SerializeField] private FusedBallProperties fusedBallStats;
    [SerializeField] private BallStatRow statWeight;
    [SerializeField] private BallStatRow statSpin;
    [SerializeField] private BallStatRow statBounce;

    private string primaryBallID;
    private string secondaryBallID;
    private bool selectingPrimary;

    private List<FusionBallButton> activePopupButtons = new List<FusionBallButton>();

    private void Start()
    {
        secondarySlotButton.interactable = false;

        primaryStatPanel.SetActive(false);
        secondaryStatPanel.SetActive(false);
        fusedStatPanel.SetActive(false);
        
        primarySlotButton.onClick.AddListener(() => OpenFusionPopup(true));
        secondarySlotButton.onClick.AddListener(() => OpenFusionPopup(false));
    }

    private void OpenFusionPopup(bool isPrimary)
    {
        selectingPrimary = isPrimary;
        fusionPopup.SetActive(true);

        foreach (var btn in activePopupButtons)
        {
            btn.Cleanup();
            Destroy(btn.gameObject);
        }
        activePopupButtons.Clear();

        IEnumerable<BallProperties> candidates = Balls.Instance.allBalls;
        if (!isPrimary && !string.IsNullOrEmpty(primaryBallID))
        {
            var primaryAxis = Balls.Instance.GetBall(primaryBallID).abilityAxis;
            candidates = candidates.Where(b => b.abilityAxis != primaryAxis);
        }

        foreach (var props in candidates)
        {
            var entry = Instantiate(fusionBallButtonPrefab, popupContentParent);
            entry.Initialize(props, OnBallSelected);
            activePopupButtons.Add(entry);
        }
    }

    private void OnBallSelected(string ballID)
    {
        if (selectingPrimary)
        {
            PrimarySelected(ballID);
        }
        else
        {
            SecondarySelected(ballID);
        }

        fusionPopup.SetActive(false);

        if (!string.IsNullOrEmpty(primaryBallID) &&
            !string.IsNullOrEmpty(secondaryBallID))
        {
            UpdateFusionStats();
        }
    }

    private void ClearStatsDisplay()
    {
        statWeight.propertyValueText.text = "-";
        statWeight.propertySlider.value   = 0f;
        statSpin.propertyValueText.text   = "-";
        statSpin.propertySlider.value     = 0f;
        statBounce.propertyValueText.text = "-";
        statBounce.propertySlider.value   = 0f;
    }

    private void UpdateFusionStats()
    {
        var a = Balls.Instance.GetBall(primaryBallID);
        var b = Balls.Instance.GetBall(secondaryBallID);

        float combinedWeight = a.weight + b.weight;
        float combinedSpin   = Mathf.Min(a.spin, b.spin);
        float combinedBounce = (a.physicsMaterial.bounciness
                              + b.physicsMaterial.bounciness) * 0.5f;

        statWeight.propertyTypeText.text  = "WEIGHT";
        statWeight.propertySlider.value   =
            Mathf.Clamp01(combinedWeight / Balls.Instance.maxWeight);
        statWeight.propertyValueText.text =
            combinedWeight.ToString("F1");

        statSpin.propertyTypeText.text    = "SPIN";
        statSpin.propertySlider.value     =
            Mathf.Clamp01(combinedSpin / Balls.Instance.maxSpin);
        statSpin.propertyValueText.text   =
            combinedSpin.ToString("F1");

        statBounce.propertyTypeText.text  = "BOUNCE";
        statBounce.propertySlider.value   =
            Mathf.Clamp01(combinedBounce / Balls.Instance.maxBounce);
        statBounce.propertyValueText.text =
            combinedBounce.ToString("F2");
    }

    public void PrimarySelected(string ballID)
    {
        primaryStatPanel.SetActive(true);
        
        primaryBallID = ballID;
        BallProperties primaryBallProperties = Balls.Instance.GetBall(primaryBallID);
        primaryBallStats.Set(primaryBallProperties, "PRIMARY");
            
        secondaryBallID = null;
        secondaryBallStats.Reset();
        secondaryStatPanel.SetActive(false);
        fusedStatPanel.SetActive(false);

        secondarySlotButton.interactable = true;
    }

    public void SecondarySelected(string ballID)
    {
        secondaryStatPanel.SetActive(true);
        fusedStatPanel.SetActive(true);
        
        secondaryBallID = ballID;
        BallProperties secondaryBallProperties = Balls.Instance.GetBall(secondaryBallID);
        secondaryBallStats.Set(secondaryBallProperties, "SECONDARY");
        
        BallProperties primaryBallProperties = Balls.Instance.GetBall(primaryBallID);
        fusedBallStats.Set(primaryBallProperties, secondaryBallProperties);
    }
}
