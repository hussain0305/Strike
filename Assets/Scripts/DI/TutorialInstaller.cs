using UnityEngine;
using Zenject;

public class TutorialInstaller : MonoInstaller
{
    [Header("Scene-Placed Managers")]
    public LevelManager levelManager;

    public override void InstallBindings()
    {
        Container
            .Bind<LevelManager>()
            .FromInstance(levelManager)
            .AsSingle();
        
        var gameStateManager = ProjectContext.Instance.Container.Resolve<GameStateManager>();
        Container.Inject(gameStateManager);
    }
}
