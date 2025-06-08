using UnityEngine;
using Zenject;

public class GameLevelInstaller : MonoInstaller
{
    [Header("Scene-Placed Managers")]
    public GameManager gameManager;
    public RoundDataManager roundDataManager;
    public CameraController cameraManager;
    public FlavorTextSpawner flavorTextSpawner;
    public GameMode gameMode;
    public NotableEventsManager notableEventsManager;
    public EffectsManager effectsManager;
    public ShotInput shotInput;
    public PlayerHUD playerHUD;
    public TrajectoryHistoryViewer trajectoryHistoryViewer;
    
    public override void InstallBindings()
    {
        Container
            .Bind<InGameContext>()
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<ShotInput>()
            .FromInstance(shotInput)
            .AsSingle();
        
        Container
            .BindInterfacesAndSelfTo<PlayerHUD>()
            .FromInstance(playerHUD)
            .AsSingle();
        
        Container
            .BindInterfacesAndSelfTo<GameManager>()
            .FromInstance(gameManager)
            .AsSingle();
        
        Container
            .Bind<RoundDataManager>()
            .FromInstance(roundDataManager)
            .AsSingle();
        
        Container
            .Bind<CameraController>()
            .FromInstance(cameraManager)
            .AsSingle();
        
        Container
            .Bind<FlavorTextSpawner>()
            .FromInstance(flavorTextSpawner)
            .AsSingle();
        
        Container
            .Bind<GameMode>()
            .FromInstance(gameMode)
            .AsSingle();
        
        Container
            .Bind<NotableEventsManager>()
            .FromInstance(notableEventsManager)
            .AsSingle();
        
        Container
            .Bind<EffectsManager>()
            .FromInstance(effectsManager)
            .AsSingle();
        
        Container
            .Bind<TrajectoryHistoryViewer>()
            .FromInstance(trajectoryHistoryViewer)
            .AsSingle();
        
        Container.Inject(gameManager);
        Container.Inject(roundDataManager);
        Container.Inject(cameraManager);
        Container.Inject(flavorTextSpawner);
        Container.Inject(gameMode);
        Container.Inject(notableEventsManager);
        Container.Inject(shotInput);
        Container.Inject(playerHUD);
        
        var poolingManager = ProjectContext.Instance.Container.Resolve<PoolingManager>();
        Container.Inject(poolingManager);
    }
}
