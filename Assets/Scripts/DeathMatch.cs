using System;
using System.Collections;
using UnityEngine;
using Zenject;

public class DeathMatch : GameMode
{
    private Coroutine nextShotCoroutine;
    
    private GameManager gameManager;
    private RoundDataManager roundDataManager;

    [Inject]
    public void Construct(GameManager _gameManager, RoundDataManager _roundDataManager)
    {
        gameManager = _gameManager;
        roundDataManager = _roundDataManager;
    }

    private void Start()
    {
        defaultWinCondition = WinCondition.Survival;
        pinBehaviour = PinBehaviourPerTurn.DisappearUponCollection;
    }

    public override void OnShotComplete(bool hitSomething)
    {
        if (hitSomething)
        {
            FlagNextShot(0);
        }
        else
        {
            int player = gameManager.CurrentPlayerTurn;
            roundDataManager.EliminatePlayer(player);
            FlagNextShot(1);
        }
    }

    private void FlagNextShot(float delay)
    {
        if (nextShotCoroutine != null)
        {
            StopCoroutine(nextShotCoroutine);
        }
        nextShotCoroutine = StartCoroutine(DelayedCueNextShot(delay));
    }
    
    IEnumerator DelayedCueNextShot(float delay)
    {
        yield return null;
        yield return new WaitForSeconds(delay);
            
        EventBus.Publish(new CueNextShotEvent());
    }

    public override WinCondition GetWinCondition()
    {
        defaultWinCondition = WinCondition.Survival;
        return base.GetWinCondition();
    }
}