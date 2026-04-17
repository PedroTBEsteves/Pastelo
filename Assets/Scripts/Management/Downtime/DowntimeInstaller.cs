using Reflex.Attributes;
using Reflex.Core;
using UnityEngine;

public class DowntimeInstaller : MonoBehaviour, IInstaller
{
    [Inject]
    private readonly Store _store;
    
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder
            .AddSingleton(Resources.Load("Settings/Management/StoreSettings"))
            .AddSingleton(typeof(Store));
    }
}
