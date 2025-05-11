using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RandomizerModePage : MonoBehaviour
{
    public Button randomizerButton;
    
    private void OnEnable()
    {
        randomizerButton.onClick.AddListener(LoadRandomizerLevel);
    }

    private void OnDisable()
    {
        randomizerButton.onClick.RemoveAllListeners();
    }

    public void LoadRandomizerLevel()
    {
        SceneManager.LoadScene(ModeSelector.Instance.GetRandomizerLevel());
    }
}
