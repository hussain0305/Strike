using Zenject;
using UnityEngine;

public class GlobalInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container
            .Bind<AudioManager>()
            .FromComponentInHierarchy()
            .AsSingle()
            .NonLazy();

        Container
            .Bind<ModeSelector>()
            .FromComponentInHierarchy()
            .AsSingle()
            .NonLazy();

        Container
            .Bind<PoolingManager>()
            .FromNewComponentOnNewGameObject()
            .AsSingle()
            .NonLazy();

        Container
            .Bind<GameStateManager>()
            .FromNewComponentOnNewGameObject()
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<MenuManager>()
            .FromNewComponentOnNewGameObject()
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<InputManager>()
            .FromNewComponentOnNewGameObject()
            .AsSingle()
            .NonLazy();
    }
}