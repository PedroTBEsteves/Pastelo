using KBCore.Refs;
using Reflex.Core;
using UnityEngine;

public class UiInstaller : ValidatedMonoBehaviour, IInstaller
{
    [SerializeField, Scene]
    private TrashBin _trashBin;
    
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder.AddScoped(_ => _trashBin, typeof(TrashBin));
    }
}
