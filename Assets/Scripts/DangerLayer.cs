using System.Collections.Generic;
using UnityEngine;

public class InitializeDangerTokens
{
    public GameObject[] dangerTokens;
    public int highestTokenIndexToActivate;

    public InitializeDangerTokens(GameObject[] _dangerTokens, int _highestTokenIndexToActivate)
    {
        dangerTokens = _dangerTokens;
        highestTokenIndexToActivate = _highestTokenIndexToActivate;
    }
}

public class DangerLayer : MonoBehaviour
{
    public GameObject[] dangerTokens;
    
    private int nextTokenToActivate = 0;
    
    private void OnEnable()
    {
        EventBus.Subscribe<NewRoundStartedEvent>(NewRoundStarted);
        EventBus.Subscribe<InitializeDangerTokens>(InitializeDangerTokens);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<NewRoundStartedEvent>(NewRoundStarted);
        EventBus.Unsubscribe<InitializeDangerTokens>(InitializeDangerTokens);
    }

    public void InitializeDangerTokens(InitializeDangerTokens e)
    {
        dangerTokens = e.dangerTokens;
        for (int i = 0; i < dangerTokens.Length; i++)
        {
            dangerTokens[i].SetActive(i <= e.highestTokenIndexToActivate);
        }
        nextTokenToActivate = e.highestTokenIndexToActivate + 1;
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
