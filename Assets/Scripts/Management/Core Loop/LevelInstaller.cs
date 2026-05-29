using Reflex.Core;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

public class LevelInstaller : MonoBehaviour, IInstaller
{
    [SerializeField]
    private Recipe _tutorialRecipe;
    
    [SerializeField]
    private LocalizedStringTable _customerOrderExpiredDialoguesTableName;
    
    [SerializeField]
    private LocalizedStringTable _customerGaveUpDialoguesTableName;

    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        var tutorialState = new GameplayTutorialState(_tutorialRecipe);

        containerBuilder
            .AddScoped(typeof(GameplayTutorialEvents))
            .AddScoped(_ => tutorialState, typeof(GameplayTutorialState))
            .AddScoped(typeof(TutorialTargetRegistry))
            .AddScoped(typeof(GameplayInteractionGate))
            .AddScoped(typeof(LevelMoneyManager))
            .AddScoped(typeof(LevelFlowController), typeof(LevelFlowController), typeof(ITickable))
            .AddScoped(typeof(TimeController))
            .AddScoped(typeof(LevelPerformanceTracker))
            .AddSingleton(Resources.Load("Settings/Management/OrderLoopSettings"))
            .AddScoped(container => new OrderController(
                    container.Resolve<OrderLoopSettings>(),
                    container.Resolve<RecipeGenerator>(),
                    container.Resolve<PastelCookingSettings>(),
                    container.Resolve<ICustomerPopUpDialogue>(),
                    _customerOrderExpiredDialoguesTableName,
                    container.Resolve<GameplayTutorialState>()),
                typeof(OrderController),
                typeof(ITickable))
            .AddScoped(container => new CustomerQueue(
                    container.Resolve<OrderLoopSettings>(),
                    container.Resolve<CustomersDatabase>(),
                    container.Resolve<ICustomerPopUpDialogue>(),
                    _customerGaveUpDialoguesTableName,
                    container.Resolve<GameplayTutorialState>(),
                    container.Resolve<LevelSelector>()),
                typeof(CustomerQueue),
                typeof(ITickable))
            .AddScoped(typeof(GameplayTutorialController));
    }
}
