using Reflex.Core;
using UnityEngine;

public class ApplicationInstaller : MonoBehaviour, IInstaller
{
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder.AddScoped(typeof(ApplicationController));
    }
}
