using UnityEngine;
using Zenject;

public class MainMenuInstaller : MonoInstaller
{
    [Header("Scene-Placed Managers")]
    public ModeSelectorUI modeSelectorUI;

    public override void InstallBindings()
    {
        Container
            .Bind<ModeSelectorUI>()
            .FromInstance(modeSelectorUI)
            .AsSingle()
            .NonLazy();
        
        var gameStateManager = Container.Resolve<GameStateManager>();
        Container.Inject(gameStateManager);
    }
}