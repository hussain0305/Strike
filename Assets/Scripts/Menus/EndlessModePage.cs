using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndlessModePage : MonoBehaviour
{
    public TextMeshProUGUI numPlayersText;

    public Button randomizerButton;
    public RandomizerRangedParameter difficulty;
    public RandomizerBoolParameter  hasDangerPins;
    public RandomizerEnumParameter  pinBehaviour;

    [Header("Record")]
    public TextMeshProUGUI numPlayedValue;
    public TextMeshProUGUI winPercentageValue;
    
    [Header("Star Info")]
    public GameObject lowerDifficultySection;
    public GameObject higherDifficultySection;
    public TextMeshProUGUI higherDifficultyText;
    
    private readonly RandomizerParameterHub hub = new();

    private void OnEnable()
    {
        difficulty.OnValueChanged += DifficultyChanged;
        
        randomizerButton.onClick.AddListener(OnGeneratePressed);
        EventBus.Subscribe<NumPlayersChangedEvent>(NumPlayersChanged);
        numPlayersText.text = ModeSelector.Instance.GetNumPlayers().ToString();
        ModeSelector.Instance.EndlessModeOpened();
        UpdateRecordUI(difficulty.ValueInt);
    }

    private void OnDisable()
    {
        randomizerButton.onClick.RemoveAllListeners();
        EventBus.Unsubscribe<NumPlayersChangedEvent>(NumPlayersChanged);
        
        difficulty.OnValueChanged -= DifficultyChanged;
    }

    void Awake()
    {
        hub.Register(difficulty);
        hub.Register(hasDangerPins);
        hub.Register(pinBehaviour);
    }

    public void OnGeneratePressed()
    {
        ModeSelector.Instance.SetRandomizerGenerationSettings(hub.ToSettings());
        SceneManager.LoadScene(ModeSelector.Instance.GetEndlessLevel());
    }

    public void NumPlayersChanged(NumPlayersChangedEvent e)
    {
        numPlayersText.text = e.numPlayers.ToString();
    }

    private void DifficultyChanged(object newValue)
    {
        if (newValue is int diff)
        {
            UpdateRecordUI(diff);
        }
    }

    private void UpdateRecordUI(int difficultyTier)
    {
        if (!SaveManager.IsSaveLoaded)
            return;
        
        var record = SaveManager.GetEndlessRecord(difficultyTier);

        numPlayedValue.text = record.numPlayed.ToString();
        string winPct = "0";
        if (record.numPlayed > 0)
        {
            float pct = (record.numWon / (float)record.numPlayed) * 100f;
            winPct = pct.ToString("F1");
        }
        winPercentageValue.text = winPct;

        bool isEasy = difficultyTier <= 5;
        lowerDifficultySection.SetActive(isEasy);
        higherDifficultySection.SetActive(!isEasy);
        if (!isEasy)
        {
            int stars = Mathf.Clamp((difficultyTier - 6) / 2 + 1, 1, 3);
            // string starWord = stars == 1 ? "star" : "stars";
            higherDifficultyText.text = $"{stars}";// {starWord} available for collection
        }
    }
}
