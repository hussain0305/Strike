using UnityEngine;

public interface ICollectibleHitReaction
{
    void UpdatePoints(int points);
    void CheckIfHitsExhasuted(int numTimesCollected, int numTimesCanBeCollected);
    void SetDefaultVisuals(NextShotCuedEvent e);
}
