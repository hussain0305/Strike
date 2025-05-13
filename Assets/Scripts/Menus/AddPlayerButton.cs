using UnityEngine;
using UnityEngine.UI;

public class AddPlayerButton : MonoBehaviour
{
    private Button button;
    private Button Button
    {
        get
        {
            if (button == null)
                button = GetComponentInParent<Button>();
            
            return button;
        }
    }
    
    private void OnEnable()
    {
        Button.onClick.AddListener(() =>
        {
            ModeSelector.Instance.AddPlayer();
        });
    }

    private void OnDisable()
    {
        Button.onClick.RemoveAllListeners();
    }
}
