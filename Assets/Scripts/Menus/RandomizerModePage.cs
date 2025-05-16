using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RandomizerModePage : MonoBehaviour
{
    public TextMeshProUGUI numPlayersText;

    public Button randomizerButton;
    public RandomizerRangedParameter difficulty;
    public RandomizerBoolParameter  hasDangerPins;
    public RandomizerEnumParameter  pinBehaviour;
    
    private readonly RandomizerParameterHub hub = new();

    private void OnEnable()
    {
        randomizerButton.onClick.AddListener(OnGeneratePressed);
        EventBus.Subscribe<NumPlayersChangedEvent>(NumPlayersChanged);
        numPlayersText.text = ModeSelector.Instance.GetNumPlayers().ToString();
        ModeSelector.Instance.GauntletModeOpened();
    }

    private void OnDisable()
    {
        randomizerButton.onClick.RemoveAllListeners();
        EventBus.Unsubscribe<NumPlayersChangedEvent>(NumPlayersChanged);
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
}
