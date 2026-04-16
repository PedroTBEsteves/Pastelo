using Reflex.Core;
using UnityEngine;

public class ManagementInstaller : MonoBehaviour, IInstaller
{
    [SerializeField]
    private Recipe _tutorialRecipe;
    
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        var state = new GameplayTutorialState(_tutorialRecipe);
        
        containerBuilder
            .AddScoped(typeof(GameplayTutorialEvents))
            .AddScoped(_ => state, typeof(GameplayTutorialState))
            .AddScoped(typeof(TutorialTargetRegistry))
            .AddScoped(typeof(GameplayInteractionGate))
            .AddSingleton(Resources.Load("Settings/Management/IngredientsStorageSettings"))
            .AddScoped(typeof(IngredientsStorage))
            .AddSingleton(Resources.Load("Settings/Management/MoneySettings"))
            .AddScoped(typeof(MoneyManager))
            .AddScoped(typeof(LevelMoneyManager))
            .AddScoped(typeof(LevelFlowController))
            .AddScoped(typeof(TimeController))
            .AddSingleton(Resources.Load("Settings/Management/OrderLoopSettings"))
            .AddScoped(typeof(OrderController), typeof(OrderController), typeof(ITickable))
            .AddScoped(typeof(CustomerQueue), typeof(CustomerQueue), typeof(ITickable))
            .AddScoped(typeof(GameplayTutorialController));
    }
}
