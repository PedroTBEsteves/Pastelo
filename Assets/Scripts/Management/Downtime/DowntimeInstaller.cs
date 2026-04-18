using Reflex.Attributes;
using Reflex.Core;
using UnityEngine;

public class DowntimeInstaller : MonoBehaviour, IInstaller
{
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder
            .AddSingleton(Resources.Load("Settings/Management/StoreSettings"))
            .AddSingleton(typeof(Store));
    }
}
