using System;
using System.Collections;
using UnityEngine;
using Zenject;

public class DeathMatch : GameMode
{
    private Coroutine nextShotCoroutine;
    
    private bool deathMatchFailCondition = false;
    
    private void Start()
    {
        defaultWinCondition = WinCondition.Survival;
        pinBehaviour = PinBehaviourPerTurn.DisappearUponCollection;
        numAdditionalVolleyGrants = 0;
    }

    public override void OnShotComplete(bool hitDangerPin, bool hitNormalPin)
    {
        deathMatchFailCondition = deathMatchFailCondition || hitDangerPin;
        bool hitSomething = hitDangerPin || hitNormalPin;
        if (hitSomething)
        {
            FlagNextShot(0);
        }
        else
        {
            deathMatchFailCondition = true;
            int player = gameManager.CurrentPlayerTurn;
            roundDataManager.EliminatePlayer(player, EliminationReason.HitNoting);
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

    public override bool LevelCompletedSuccessfully()
    {
        return !deathMatchFailCondition;
    }
}