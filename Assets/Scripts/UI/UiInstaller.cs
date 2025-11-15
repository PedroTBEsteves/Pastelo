using KBCore.Refs;
using Reflex.Core;
using UnityEngine;

public class UiInstaller : ValidatedMonoBehaviour, IInstaller
{
    [SerializeField, Scene]
    private TrashBin _trashBin;
    
    [SerializeField, Scene]
    private PopupTextService _popupTextService;
    
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder
            .AddScoped(_ => _trashBin, typeof(TrashBin))
            .AddScoped(_ => _popupTextService, typeof(IPopupTextService));
    }
}
