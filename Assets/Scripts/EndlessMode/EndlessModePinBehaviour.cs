using UnityEngine;

[CreateAssetMenu(fileName = "EndlessModePinBehaviour", menuName = "Game/Endless Mode Pin Behaviour")]
public class EndlessModePinBehaviour : ScriptableObject
{
    private static EndlessModePinBehaviour _instance;
    public static EndlessModePinBehaviour Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<EndlessModePinBehaviour>("EndlessModePinBehaviour");
                if (_instance == null)
                {
                    Debug.LogError("EndlessModePinBehaviour instance not found. Please create one in the Resources folder.");
                }
            }
            return _instance;
        }
    }

    public RandomizerEnum[] pinBehaviours;
}
