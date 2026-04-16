using KBCore.Refs;
using Reflex.Core;
using UnityEngine;

public class GameplayFlowInstaller : ValidatedMonoBehaviour, IInstaller
{
    [SerializeField, Scene]
    private GameplayLoopFlowController _gameplayLoopFlowController;
    
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder
            .AddSingleton(_ => _gameplayLoopFlowController, typeof(GameplayLoopFlowController))
            .AddSingleton(typeof(LevelSelector));
    }
}
