using Reflex.Core;
using UnityEngine;

public sealed class DraggableProjectInstaller : MonoBehaviour, IInstaller
{
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder.AddSingleton(typeof(DraggableInputConfiguration));
    }
}
