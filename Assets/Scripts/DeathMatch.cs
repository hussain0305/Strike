using System;
using System.Collections;
using UnityEngine;

public class DeathMatch : GameMode
{
    private Coroutine nextShotCoroutine;
    
    private void Start()
    {
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
            int player = GameManager.Instance.CurrentPlayerTurn;
            RoundDataManager.Instance.EliminatePlayer(player);
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
        base.GetWinCondition();
        return WinCondition.Survival;
    }
}