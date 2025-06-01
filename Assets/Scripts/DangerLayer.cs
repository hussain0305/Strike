using System.Collections.Generic;
using UnityEngine;

public class InitializeDangerTokens
{
    public int NumPlayers;
    public GameObject[] DangerTokens;
    public int HighestTokenIndexToActivate;

    public InitializeDangerTokens(GameObject[] _dangerTokens, int _highestTokenIndexToActivate, int numPlayers)
    {
        DangerTokens = _dangerTokens;
        HighestTokenIndexToActivate = _highestTokenIndexToActivate;
        NumPlayers = numPlayers;
    }
}

public class DangerLayer : MonoBehaviour
{
    private GameObject[] dangerTokens;
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
        dangerTokens = e.DangerTokens;
        for (int i = 0; i < dangerTokens.Length; i++)
        {
            dangerTokens[i].SetActive(true);
            dangerTokens[i].GetComponent<Collectible>().NewGameStarted(e.NumPlayers);
            dangerTokens[i].SetActive(i <= e.HighestTokenIndexToActivate);
        }
        nextTokenToActivate = e.HighestTokenIndexToActivate + 1;
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
