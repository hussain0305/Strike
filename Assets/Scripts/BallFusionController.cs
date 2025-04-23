using System.Collections;
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
    [SerializeField] private FusionSlotButton primarySlotButton;
    [SerializeField] private FusionSlotButton secondarySlotButton;
    [SerializeField] private GameObject fusionPopup;
    [SerializeField] private Transform popupContentParent;
    [SerializeField] private FusionBallButton fusionBallButtonPrefab;

    [Header("Stat Panel")]
    [SerializeField] private GameObject primaryStatPanel;
    [SerializeField] private GameObject secondaryStatPanel;
    [SerializeField] private GameObject fusedStatPanel;
    [SerializeField] private FusionBallProperties primaryBallStats;
    [SerializeField] private FusionBallProperties secondaryBallStats;
    [SerializeField] private FusedBallProperties fusedBallStats;
    [SerializeField] private BallStatRow statWeight;
    [SerializeField] private BallStatRow statSpin;
    [SerializeField] private BallStatRow statBounce;

    [Header("Action Buttons")]
    [SerializeField] private GameObject addToFavoritesButtonObject;
    [SerializeField] private GameObject fuseButtonObject;
    [SerializeField] private GameObject equipButtonObject;
    [SerializeField] private GameObject equippedObject;
    [SerializeField] private TextMeshProUGUI fuseCostText;
    [SerializeField] private GameObject cantUnlockSection;
    
    [Header("Favorites")]
    [SerializeField] private FavoriteFusionButton[] favoriteButtons;

    private Button AddToFavoritesButton => addToFavoritesButtonObject.GetComponentInChildren<Button>();
    private Button FuseButton => fuseButtonObject.GetComponentInChildren<Button>();
    private Button EquipButton => equipButtonObject.GetComponentInChildren<Button>();

    private string primaryBallID;
    private string secondaryBallID;
    private bool   selectingPrimary;
    private List<FusionBallButton> activePopupButtons = new();
    private List<FusionBallButton> pooledButtons = new List<FusionBallButton>();
    private bool setupComplete = false;
    private Coroutine cantUnlockMessageCoroutine;

    private void Start()
    {
        secondarySlotButton.SetInteractable(false);
        primaryStatPanel.SetActive(false);
        secondaryStatPanel.SetActive(false);
        fusedStatPanel.SetActive(false);

        primarySlotButton.button.onClick.AddListener(() => OpenFusionPopup(true));
        secondarySlotButton.button.onClick.AddListener(() => OpenFusionPopup(false));

        FuseButton.onClick.AddListener(TryFuseBalls);
        AddToFavoritesButton.onClick.AddListener(OnAddToFavorites);
        EquipButton.onClick.AddListener(OnEquip);

        foreach (var props in Balls.Instance.allBalls)
        {
            var btn = Instantiate(fusionBallButtonPrefab, popupContentParent);
            btn.Initialize(props, OnBallSelected);
            btn.gameObject.SetActive(false);
            pooledButtons.Add(btn);
        }

        LoadSelectedFusion();
        LoadFavorites();
        UpdateActionButtons();
        setupComplete = true;
    }

    private void OnEnable()
    {
        if (!SaveManager.IsSaveLoaded || !setupComplete)
            return;

        LoadSelectedFusion();
        LoadFavorites();
        UpdateActionButtons();
    }
    
    private void OpenFusionPopup(bool isPrimary)
    {
        selectingPrimary = isPrimary;
        fusionPopup.SetActive(true);

        AbilityAxis? blockAxis = null;
        if (!isPrimary && !string.IsNullOrEmpty(primaryBallID))
            blockAxis = Balls.Instance.GetBall(primaryBallID).abilityAxis;

        foreach (var btn in pooledButtons)
        {
            bool allow = isPrimary
                         || blockAxis == null
                         || btn.ballProperties.abilityAxis != blockAxis.Value;
            btn.gameObject.SetActive(allow);
        }
    }

    private void OnBallSelected(string ballID)
    {
        if (selectingPrimary)
            PrimarySelected(ballID);
        else
            SecondarySelected(ballID);

        fusionPopup.SetActive(false);

        if (!string.IsNullOrEmpty(primaryBallID) && !string.IsNullOrEmpty(secondaryBallID))
            UpdateFusionStats();

        UpdateActionButtons();
    }

    private void PrimarySelected(string ballID)
    {
        primaryBallID = ballID;
        primarySlotButton.SetSelected(ballID);
        primaryStatPanel.SetActive(true);

        primaryBallStats.Set(Balls.Instance.GetBall(ballID), "PRIMARY");

        secondaryBallID = null;
        secondarySlotButton.SetUnselected();
        secondarySlotButton.SetInteractable(true);
        secondaryStatPanel.SetActive(false);
        fusedStatPanel.SetActive(false);
    }

    private void SecondarySelected(string ballID)
    {
        secondaryBallID = ballID;
        secondarySlotButton.SetSelected(ballID);
        secondaryStatPanel.SetActive(true);
        fusedStatPanel.SetActive(true);

        secondaryBallStats.Set(Balls.Instance.GetBall(ballID), "SECONDARY");
        fusedBallStats.Set(
            Balls.Instance.GetBall(primaryBallID),
            Balls.Instance.GetBall(secondaryBallID)
        );
    }

    private void UpdateFusionStats()
    {
        var a = Balls.Instance.GetBall(primaryBallID);
        var b = Balls.Instance.GetBall(secondaryBallID);

        float w = a.weight + b.weight;
        float s = Mathf.Min(a.spin, b.spin);
        float c = (a.physicsMaterial.bounciness + b.physicsMaterial.bounciness) * .5f;

        statWeight.SetWeight(w);
        statSpin.SetSpin(s);
        statBounce.SetBounce(c);
    }

    private void LoadSelectedFusion()
    {
        string key = SaveManager.GetSelectedFusion();
        if (string.IsNullOrEmpty(key) || !SaveManager.IsFusionUnlockedKey(key))
            return;

        var parts = key.Split('+');
        if (parts.Length != 2) return;

        PrimarySelected(parts[0]);
        SecondarySelected(parts[1]);
        UpdateFusionStats();
    }

    private void LoadFavorites()
    {
        for (int i = 0; i < favoriteButtons.Length; i++)
        {
            var faveKey = SaveManager.GetFavoriteFusionAt(i);
            var btn= favoriteButtons[i];
            if (!string.IsNullOrEmpty(faveKey) && SaveManager.IsFusionUnlockedKey(faveKey))
            {
                btn.gameObject.SetActive(true);
                btn.Initialize(faveKey, OnFavoriteClicked);
            }
            else
            {
                btn.Cleanup();
                btn.gameObject.SetActive(false);
            }
        }
    }

    private void OnFavoriteClicked(string fusionKey)
    {
        var parts = fusionKey.Split('+');
        if (parts.Length != 2) return;

        PrimarySelected(parts[0]);
        SecondarySelected(parts[1]);
        UpdateFusionStats();
        UpdateActionButtons();
    }

    private void OnFuse()
    {
        if (string.IsNullOrEmpty(primaryBallID) || string.IsNullOrEmpty(secondaryBallID))
            return;

        SaveManager.AddUnlockedFusion(primaryBallID, secondaryBallID);
        UpdateActionButtons();
        LoadFavorites();
    }

    private void OnAddToFavorites()
    {
        SaveManager.AddFusionToFavorites(primaryBallID, secondaryBallID);
        UpdateActionButtons();
        LoadFavorites();
    }

    private void OnEquip()
    {
        SaveManager.SetSelectedFusion(primaryBallID, secondaryBallID);
        SaveManager.SetFusionEquipped(true);
        UpdateActionButtons();
    }

    private void UpdateActionButtons()
    {
        bool bothSet = !string.IsNullOrEmpty(primaryBallID) && !string.IsNullOrEmpty(secondaryBallID);

        if (!bothSet)
        {
            fuseButtonObject.SetActive(false);
            addToFavoritesButtonObject.SetActive(false);
            equipButtonObject.SetActive(false);
            equippedObject.SetActive(false);
            cantUnlockSection.SetActive(false);
            return;
        }

        string key = $"{primaryBallID}+{secondaryBallID}";
        bool unlocked = SaveManager.IsFusionUnlocked(primaryBallID, secondaryBallID);
        bool favorited = SaveManager.IsFusionFavorited(primaryBallID, secondaryBallID);
        bool equipped = SaveManager.IsFusionEquipped() && SaveManager.GetSelectedFusion() == key;

        fuseButtonObject.SetActive(!unlocked);
        addToFavoritesButtonObject.SetActive(unlocked && !favorited);
        equipButtonObject.SetActive(unlocked && !equipped);
        fuseCostText.text = Balls.Instance.GetFusionCost(primaryBallID, secondaryBallID).ToString();
        equippedObject.SetActive(unlocked && equipped);
        cantUnlockSection.SetActive(false);
    }
    
    public void TryFuseBalls()
    {
        if (string.IsNullOrEmpty(primaryBallID) || string.IsNullOrEmpty(secondaryBallID))
            return;

        int cost = Balls.Instance.GetFusionCost(primaryBallID, secondaryBallID);
        if (cost <= SaveManager.GetStars())
        {
            SaveManager.SpendStars(cost);
            SaveManager.AddUnlockedFusion(primaryBallID, secondaryBallID);
            UpdateActionButtons();
            LoadFavorites();
        }
        else
        {
            CancelOngoingProcesses();
            cantUnlockMessageCoroutine = StartCoroutine(ShowCannotUnlockMessage());
        }
    }

    public void CancelOngoingProcesses()
    {
        if (cantUnlockMessageCoroutine != null)
        {
            StopCoroutine(cantUnlockMessageCoroutine);
        }
    }

    private IEnumerator ShowCannotUnlockMessage()
    {
        fuseButtonObject.SetActive(false);
        addToFavoritesButtonObject.SetActive(false);
        equipButtonObject.SetActive(false);
        equippedObject.SetActive(false);
        cantUnlockSection.gameObject.SetActive(true);

        cantUnlockSection.gameObject.SetActive(true);

        yield return new WaitForSeconds(2);

        UpdateActionButtons();
        cantUnlockMessageCoroutine = null;
    }

}