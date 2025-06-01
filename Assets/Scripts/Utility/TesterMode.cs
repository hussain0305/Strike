using System;
using UnityEngine;
using UnityEngine.UI;

public class TesterMode : MonoBehaviour
{
    [Header("DEBUG")]
    [SerializeField] Button addStars;
    [SerializeField] Button deductStars;
    [SerializeField] Button unlockAllLevels;

    private bool testerMode = false;

    private void OnEnable()
    {
#if TESTER_MODE
        testerMode = true;
        addStars.onClick.AddListener(() => SaveManager.AddStars(40));
        deductStars.onClick.AddListener(() => SaveManager.SpendStars(20));
        unlockAllLevels.onClick.AddListener(UnlockAllLevels);
#else
        Destroy(gameObject);
#endif
    }

    private void OnDisable()
    {
        addStars.onClick.RemoveAllListeners();
        deductStars.onClick.RemoveAllListeners();
        unlockAllLevels.onClick.RemoveAllListeners();
    }

    private void UnlockAllLevels()
    {
        foreach (var gameMode in GameModeLevelMapping.Instance.gameModeLevels)
        {
            foreach (int level in gameMode.levels)
            {
                SaveManager.SetLevelCompleted(gameMode.gameMode, level);
            }
        }
    }
}
