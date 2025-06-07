using System;
using UnityEngine;

public class Switch : MonoBehaviour
{
    private ISwitchable switchable;
    private ISwitchable Switchable => switchable ??= GetComponentInParent<ISwitchable>();

    private MeshRenderer mesh;
    private MeshRenderer Mesh => mesh ??= GetComponentInChildren<MeshRenderer>();
    
    private bool switchedOn = false;
    private bool switchedThisTurn = false;
    
    private int currentPlayerTurn = 0;
    private bool[] stateForPlayers;

    private void OnEnable()
    {
        EventBus.Subscribe<NextShotCuedEvent>(NextShotCued);
        EventBus.Subscribe<NewGameStartedEvent>(NewGameStarted);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<NextShotCuedEvent>(NextShotCued);
        EventBus.Unsubscribe<NewGameStartedEvent>(NewGameStarted);
    }

    public void NewGameStarted(NewGameStartedEvent e)
    {
        currentPlayerTurn = 0;
        stateForPlayers = new bool[e.NumPlayers];
        Reset();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (switchedThisTurn)
            return;

        Obstacle triggeringObstacle = other.GetComponent<Obstacle>();
        bool markAsSwitched = triggeringObstacle && triggeringObstacle.type != ObstacleType.SwitchFlipper;
        SwitchPressed(markAsSwitched);
    }

    public void SwitchPressed(bool markAsSwitched)
    {
        if(markAsSwitched)
            switchedThisTurn = true;
        
        switchedOn = !switchedOn;
        Mesh.sharedMaterial = switchedOn ? GlobalAssets.Instance.switchedOnMaterial : GlobalAssets.Instance.switchedOffMaterial;
        Switchable.Switched(switchedOn);
        stateForPlayers[currentPlayerTurn] = switchedOn;
    }

    public void SetSwitchOn()
    {
        switchedOn = true;
        Mesh.sharedMaterial = switchedOn ? GlobalAssets.Instance.switchedOnMaterial : GlobalAssets.Instance.switchedOffMaterial;
    }

    public void SetSwitchOff()
    {
        switchedOn = false;
        Mesh.sharedMaterial = switchedOn ? GlobalAssets.Instance.switchedOnMaterial : GlobalAssets.Instance.switchedOffMaterial;
    }

    public void Reset()
    {
        switchedThisTurn = false;
        SetSwitchOff();
        Switchable.Reset();
    }

    public void SyncForPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= stateForPlayers.Length)
            return;

        switchedThisTurn = false;
        switchedOn = stateForPlayers[playerIndex];
        
        Switchable.SyncForPlayer(switchedOn);
        
        if (switchedOn)
            SetSwitchOn();
        else
            SetSwitchOff();
    }
    
    public void NextShotCued(NextShotCuedEvent e)
    {
        switchedThisTurn = false;
        currentPlayerTurn = e.CurrentPlayerTurn;
        SyncForPlayer(currentPlayerTurn);
    }
}
