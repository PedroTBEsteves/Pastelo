using Reflex.Core;
using UnityEngine;

public class ManagementInstaller : MonoBehaviour, IInstaller
{
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder
            .AddSingleton(Resources.Load("Settings/Management/IngredientsStorageSettings"))
            .AddScoped(typeof(IngredientsStorage))
            .AddSingleton(Resources.Load("Settings/Management/MoneySettings"))
            .AddScoped(typeof(Money))
            .AddScoped(typeof(TimeController))
            .AddSingleton(Resources.Load("Settings/Management/OrderLoopSettings"))
            .AddScoped(typeof(OrderController), typeof(OrderController), typeof(ITickable))
            .AddScoped(typeof(CustomerQueue), typeof(CustomerQueue), typeof(ITickable))
            .AddScoped(typeof(StrikesController));
    }
}
