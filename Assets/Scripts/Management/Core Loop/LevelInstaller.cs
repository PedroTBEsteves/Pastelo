using Reflex.Core;
using UnityEngine;

public class LevelInstaller : MonoBehaviour, IInstaller
{
    [SerializeField]
    private Recipe _tutorialRecipe;

    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        var tutorialState = new GameplayTutorialState(_tutorialRecipe);

        containerBuilder
            .AddScoped(typeof(GameplayTutorialEvents))
            .AddScoped(_ => tutorialState, typeof(GameplayTutorialState))
            .AddScoped(typeof(TutorialTargetRegistry))
            .AddScoped(typeof(GameplayInteractionGate))
            .AddScoped(typeof(LevelMoneyManager))
            .AddScoped(typeof(LevelFlowController))
            .AddScoped(typeof(TimeController))
            .AddSingleton(Resources.Load("Settings/Management/OrderLoopSettings"))
            .AddScoped(typeof(OrderController), typeof(OrderController), typeof(ITickable))
            .AddScoped(typeof(CustomerQueue), typeof(CustomerQueue), typeof(ITickable))
            .AddScoped(typeof(GameplayTutorialController));
    }
}
