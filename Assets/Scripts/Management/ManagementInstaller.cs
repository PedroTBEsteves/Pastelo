using Reflex.Core;
using UnityEngine;

public class ManagementInstaller : MonoBehaviour, IInstaller
{
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder
            .AddSingleton(Resources.Load("Settings/Management/IngredientsStorageSettings"))
            .AddSingleton(typeof(IngredientsStorage))
            .AddSingleton(Resources.Load("Settings/Management/MoneySettings"))
            .AddSingleton(typeof(MoneyManager));
    }
}
