using UnityEngine;
using Zenject;

public class GameLevelInstaller : MonoInstaller
{
    [Header("Scene-Placed Managers")]
    public GameManager gameManager;
    public RoundDataManager roundDataManager;
    public CameraController cameraManager;
    public QOLFeaturesManager qolFeaturesManager;
    public FlavorTextSpawner flavorTextSpawner;
    public GameMode gameMode;
    public NotableEventsManager notableEventsManager;
    public EffectsManager effectsManager;
    
    public override void InstallBindings()
    {
        Container
            .Bind<GameManager>()
            .FromInstance(gameManager)
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<RoundDataManager>()
            .FromInstance(roundDataManager)
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<CameraController>()
            .FromInstance(cameraManager)
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<QOLFeaturesManager>()
            .FromInstance(qolFeaturesManager)
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<FlavorTextSpawner>()
            .FromInstance(flavorTextSpawner)
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<GameMode>()
            .FromInstance(gameMode)
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<NotableEventsManager>()
            .FromInstance(notableEventsManager)
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<EffectsManager>()
            .FromInstance(effectsManager)
            .AsSingle()
            .NonLazy();
    }

}
