using UnityEngine;
using UnityEngine.UI;

public class RandomizerParamDecrementButton : MonoBehaviour
{
    private RandomizerRangedParameter parameter;
    private RandomizerRangedParameter Parameter
    {
        get
        {
            if(parameter == null)
                parameter = GetComponentInParent<RandomizerRangedParameter>();
            return parameter;
        }
    }

    private Button button;
    private Button Button
    {
        get
        {
            if (button == null)
                button = GetComponent<Button>();
            
            return button;
        }
    }
    
    private void OnEnable()
    {
        Button.onClick.AddListener(Clicked);
    }

    private void OnDisable()
    {
        Button.onClick.RemoveAllListeners();
    }

    public void Clicked()
    {
        Parameter?.Decrement();
    }
}
