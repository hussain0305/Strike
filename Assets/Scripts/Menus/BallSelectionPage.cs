using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Zenject;

public class BallSelectionPage : MonoBehaviour
{
    [Inject]
    public DiContainer diContainer;
    
    public Transform previewBallsParent;
    public BallSelectionButton ballSelectionPrefab;
    public Transform ballSelectionButtonsParent;
    public BallStatRow statWeight;
    public BallStatRow statSpin;
    public BallStatRow statBounce;
    public TextMeshProUGUI ballDescription;
    public BallPreviewController previewController;

    [Header("Rarity")]
    public Image[] frame;
    public TextMeshProUGUI rarityText;

    [Header("Equip")]
    public GameObject equipSection;
    public Button equipBallButton;
    public GameObject equippedSection;
    
    [Header("Unlock")]
    public GameObject unlockSection;
    public Button unlockBallButton;
    public TextMeshProUGUI unlockCostText;
    public GameObject cantUnlockSection;

    [Header("Info Panel Master Sections")]
    public GameObject noBallSelectedSection;
    public GameObject someBallSelectedSection;
    
    private Dictionary<string, GameObject> previewBalls;
    private Dictionary<string, BallSelectionButton> ballButtons;

    private string currentSelectedBall;
    private bool setupComplete = false;
    
    private Coroutine cantUnlockMessageCoroutine;
    
    public void Start()
    {
        SaveManager.RegisterListener(this);
        EventBus.Subscribe<SaveLoadedEvent>(SetupBallSelection);
    }

    private void OnEnable()
    {
        EventBus.Subscribe<BallSelectedEvent>(BallSelected);
        
        equipBallButton.onClick.AddListener(OnEquipBall);        
        unlockBallButton.onClick.AddListener(TryUnlockBall);

        if (!SaveManager.IsSaveLoaded || !setupComplete)
            return;

        if (!setupComplete)
            SetupBallSelection(null);
        
        if (SaveManager.IsFusionEquipped())
        {
            foreach (var btn in ballButtons.Values)
                btn.SetUnselected();

            noBallSelectedSection.SetActive(true);
            someBallSelectedSection.SetActive(false);
        }
        else
        {
            noBallSelectedSection.SetActive(false);
            someBallSelectedSection.SetActive(true);
            SetSelectedBall(SaveManager.GetEquippedBall());
        }
    }

    private void OnDisable()
    {
        EventBus.Publish(new MenuClosedEvent(MenuBase.MenuType.BallSelectionPage));
        EventBus.Unsubscribe<BallSelectedEvent>(BallSelected);
        
        equipBallButton.onClick.RemoveAllListeners();
        unlockBallButton.onClick.RemoveAllListeners();
    }

    private void OnEquipBall()
    {
        EventBus.Publish(new PlaySoundEvent(SFXType.MenuActionPositive));
        SaveManager.SetEquippedBall(currentSelectedBall);
        SetupEquipOrUnlockButton();
    }
    
    public void SetupBallSelection(SaveLoadedEvent e)
    {
        previewBalls = new Dictionary<string, GameObject>();
        ballButtons = new Dictionary<string, BallSelectionButton>();
        SpawnButtonsAndSetSelected();
        EventBus.Unsubscribe<SaveLoadedEvent>(SetupBallSelection);
        setupComplete = true;
        SaveManager.MarkListenerComplete(this);
    }

    public void SpawnButtonsAndSetSelected()
    {
        int i = 0;
        foreach (BallProperties ballProperty in Balls.Instance.allBalls)
        {
            BallSelectionButton spawnedSelectionButton = Instantiate(ballSelectionPrefab, ballSelectionButtonsParent);
            spawnedSelectionButton.SetBallName(ballProperty.name);
            spawnedSelectionButton.SetBallIndex(i);
            spawnedSelectionButton.SetBallID(ballProperty.id);
            diContainer.InjectGameObject(spawnedSelectionButton.gameObject);
            ballButtons.Add(ballProperty.id, spawnedSelectionButton);
            i++;
        }

        if (!SaveManager.IsFusionEquipped())
        {
            SetSelectedBall(SaveManager.GetEquippedBall());
        }
    }

    public void BallSelected(BallSelectedEvent e)
    {
        SetSelectedBall(e.ID);
    }
    
    public void SetSelectedBall(string ballID)
    {
        CancelOngoingProcesses();
        
        noBallSelectedSection.SetActive(false);
        someBallSelectedSection.SetActive(true);
        
        currentSelectedBall = ballID;
        GameObject selectedBall = SetupBallForPreview(ballID);

        BallProperties selectedBallProperties = Balls.Instance.GetBall(ballID);
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
        
        HighlightSelected(ballID);
        SetupEquipOrUnlockButton();

        if (gameObject.activeInHierarchy)
        {
            previewController.PlayPreview(selectedBallProperties.id, selectedBall);
        }
    }
    
    public void HighlightSelected(string ballID)
    {
        foreach (string bID in ballButtons.Keys)
        {
            if (bID == ballID)
            {
                ballButtons[ballID].SetSelected();
            }
            else
            {
                ballButtons[bID].SetUnselected();
            }
        }
    }

    public GameObject SetupBallForPreview(string ballID)
    {
        GameObject selectedBall = null;
        foreach (string bID in previewBalls.Keys)
        {
            previewBalls[bID].SetActive(false);
            if (bID == ballID)
            {
                selectedBall = previewBalls[ballID];
                selectedBall.SetActive(true);
            }
        }

        if (!selectedBall)
        {
            selectedBall = Instantiate(Balls.Instance.GetBall(ballID).prefab, previewBallsParent);
            diContainer.InjectGameObject(selectedBall);
            selectedBall.transform.localScale *= 0.25f;
            previewBalls.Add(ballID, selectedBall);
        }

        selectedBall.transform.position = previewController.tee.ballPosition.position;
        MainMenu.Context.InitPreview(selectedBall.GetComponent<Ball>(), previewController);
        return selectedBall;
    }

    public void SetupEquipOrUnlockButton()
    {
        bool isCurrentBallEquipped = !SaveManager.IsFusionEquipped() && SaveManager.GetEquippedBall() == currentSelectedBall;
        bool isBallUnlocked = SaveManager.IsBallUnlocked(currentSelectedBall);
        equippedSection.gameObject.SetActive(isCurrentBallEquipped);
        equipSection.gameObject.SetActive(isBallUnlocked && !isCurrentBallEquipped);
        unlockSection.gameObject.SetActive(!isBallUnlocked);
        cantUnlockSection.gameObject.SetActive(false);
        if (!isBallUnlocked)
        {
            unlockCostText.text = Balls.Instance.GetBallCost(currentSelectedBall).ToString();
        }
    }

    public void TryUnlockBall()
    {
        int cost = Balls.Instance.GetBallCost(currentSelectedBall);
        if (cost <= SaveManager.GetStars())
        {
            SaveManager.SpendStars(cost);
            SaveManager.AddUnlockedBall(currentSelectedBall);
            SetupEquipOrUnlockButton();
            EventBus.Publish(new PlaySoundEvent(SFXType.MenuActionPositive));
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
        EventBus.Publish(new PlaySoundEvent(SFXType.MenuActionNegative));
        equippedSection.gameObject.SetActive(false);
        equipSection.gameObject.SetActive(false);
        unlockSection.gameObject.SetActive(false);
        cantUnlockSection.gameObject.SetActive(true);

        yield return new WaitForSeconds(2);

        SetupEquipOrUnlockButton();
        cantUnlockMessageCoroutine = null;
    }
}
