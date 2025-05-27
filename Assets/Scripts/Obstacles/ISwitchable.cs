using UnityEngine;

public interface ISwitchable
{
    void Reset();
    void SyncForPlayer(bool open);
    void Switched(bool switchedOn);
}
