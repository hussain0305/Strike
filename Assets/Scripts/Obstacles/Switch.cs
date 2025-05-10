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
    
    private void OnEnable()
    {
        EventBus.Subscribe<NextShotCuedEvent>(NextShotCued);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<NextShotCuedEvent>(NextShotCued);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (switchedThisTurn)
            return;

        switchedThisTurn = true;
        SwitchPressed();
    }

    public void SwitchPressed()
    {
        switchedThisTurn = true;
        switchedOn = !switchedOn;
        Mesh.sharedMaterial = switchedOn ? GlobalAssets.Instance.switchedOnMaterial : GlobalAssets.Instance.switchedOffMaterial;
        Switchable.Switched(switchedOn);
    }
    
    public void NextShotCued(NextShotCuedEvent e)
    {
        switchedThisTurn = false;
    }
}
