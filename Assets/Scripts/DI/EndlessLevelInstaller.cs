using UnityEngine;
using Zenject;

public class EndlessLevelInstaller : MonoInstaller
{
    [Header("Scene-Placed Managers")]
    public GameManager gameManager;
    public RoundDataManager roundDataManager;
    public CameraController cameraManager;
    public FlavorTextSpawner flavorTextSpawner;
    public NotableEventsManager notableEventsManager;
    // public EffectsManager effectsManager;
    public ShotInput shotInput;
    public PlayerHUD playerHUD;
    public TrajectoryHistoryViewer trajectoryHistoryViewer;
    public LevelManager levelManager;

    public override void InstallBindings()
    {
        Container
            .Bind<InGameContext>()
            .AsSingle();
        
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
            .Bind<NotableEventsManager>()
            .FromInstance(notableEventsManager)
            .AsSingle();
        
        // Container
        //     .Bind<EffectsManager>()
        //     .FromInstance(effectsManager)
        //     .AsSingle();
        
        Container
            .Bind<GameMode>()
            .To<RegularMode>()
            .FromNewComponentOnNewGameObject()
            .AsSingle();
        
        Container
            .Bind<TrajectoryHistoryViewer>()
            .FromInstance(trajectoryHistoryViewer)
            .AsSingle();
        
        Container
            .Bind<LevelManager>()
            .FromInstance(levelManager)
            .AsSingle();

        var defaultMode = Container.Resolve<GameMode>();
        
        Container.Inject(gameManager);
        Container.Inject(roundDataManager);
        Container.Inject(cameraManager);
        Container.Inject(flavorTextSpawner);
        Container.Inject(notableEventsManager);
        Container.Inject(defaultMode);
        Container.Inject(shotInput);
        Container.Inject(playerHUD);
        
        var gameStateManager = ProjectContext.Instance.Container.Resolve<GameStateManager>();
        Container.Inject(gameStateManager);

        var poolingManager = ProjectContext.Instance.Container.Resolve<PoolingManager>();
        Container.Inject(poolingManager);
    }

    public void ReinjectAll()
    {
        Container.InjectGameObject(gameObject);
        
        var gameStateManager = ProjectContext.Instance.Container.Resolve<GameStateManager>();
        Container.Inject(gameStateManager);

        var poolingManager = ProjectContext.Instance.Container.Resolve<PoolingManager>();
        Container.Inject(poolingManager);
        
        var inGameContext = Container.Resolve<InGameContext>();
        Container.Inject(inGameContext);
    }
}
