using UnityEngine;

public class DangerLayer : MonoBehaviour
{
    public GameObject[] dangerTokens;
    
    private int nextTokenToActivate = 0;
    
    private void OnEnable()
    {
        EventBus.Subscribe<NewRoundStartedEvent>(NewRoundStarted);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<NewRoundStartedEvent>(NewRoundStarted);
    }

    public void NewRoundStarted(NewRoundStartedEvent e)
    {
        if (nextTokenToActivate < dangerTokens.Length)
        {
            dangerTokens[nextTokenToActivate].SetActive(true);
            nextTokenToActivate++;
        }
    }
}
