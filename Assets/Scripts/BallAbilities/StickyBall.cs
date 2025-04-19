using UnityEngine;

public class StickyBall : BallAbility
{
    private Rigidbody rBody;
    private Rigidbody Rigidbody => rBody ??= GetComponent<Rigidbody>();
    
    public void OnEnable()
    {
        base.OnEnable();
        EventBus.Subscribe<BallHitSomethingEvent>(BallHitSomething);
    }

    public void OnDisable()
    {
        base.OnDisable();
        EventBus.Unsubscribe<BallHitSomethingEvent>(BallHitSomething);
    }
    
    public override void BallShot(BallShotEvent e)
    {
        
    }

    public override void NextShotCued(NextShotCuedEvent e)
    {
        
    }

    public void BallHitSomething(BallHitSomethingEvent e)
    {
        Rigidbody.useGravity = false;
        Rigidbody.isKinematic = true;
        Rigidbody.angularVelocity = Vector3.zero;
        Rigidbody.linearVelocity = Vector3.zero;
    }
}
