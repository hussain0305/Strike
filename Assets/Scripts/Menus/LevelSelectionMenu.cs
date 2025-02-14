using System;
using UnityEngine;

public class LevelSelectionMenu : MonoBehaviour
{
    private void OnEnable()
    {
        ButtonStateManager.Instance.ResetLevelSelectionButtons();
    }
}
