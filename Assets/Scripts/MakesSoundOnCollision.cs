using System;
using UnityEngine;
using Zenject;

public class PlaySoundEvent
{
    public SFXType Type;
    public float Pitch;
    public float Volume;
    public PlaySoundEvent(SFXType _type, float _pitch = 1, float _volume = 1)
    {
        Type = _type;
        Pitch = _pitch;
        Volume = _volume;
    }
}

public class MakesSoundOnCollision : MonoBehaviour
{
    public SFXType sfxType;
    public float cooldown = 1;

    private float nextMakeSoundAt = 0;
    
    private void OnCollisionEnter(Collision other)
    {
        if (nextMakeSoundAt > Time.time)
            return;
        
        float impactSpeed = other.impulse.sqrMagnitude;
        float normalizedSpeed = Mathf.InverseLerp(0, 4000f, impactSpeed); 
        float volume = Mathf.Lerp(0.4f, 1.25f, normalizedSpeed);
        float pitch = Mathf.Lerp(0.4f, 1.25f, normalizedSpeed);
        
        EventBus.Publish(new PlaySoundEvent(sfxType, pitch, volume));
        nextMakeSoundAt = Time.time + cooldown;
    }
}
