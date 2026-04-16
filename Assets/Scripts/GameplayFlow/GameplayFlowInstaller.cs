using KBCore.Refs;
using Reflex.Core;
using UnityEngine;

public class GameplayFlowInstaller : ValidatedMonoBehaviour, IInstaller
{
    [SerializeField, Scene]
    private GameplayLoopFlowController _gameplayLoopFlowController;
    
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder.AddScoped(_ => _gameplayLoopFlowController, typeof(GameplayLoopFlowController));
    }
}
