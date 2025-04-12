using UnityEngine;
using UnityEngine.Serialization;

public class TutorialCategory : MonoBehaviour
{
    public string categoryName;
    [HideInInspector]
    public TutorialStep[] categorySteps;
    
    public void StartStep(int stepIndex, TutorialController controller)
    {
        categorySteps[stepIndex].Begin(controller);
    }
}
