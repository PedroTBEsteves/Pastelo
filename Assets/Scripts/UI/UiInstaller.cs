using KBCore.Refs;
using Reflex.Core;
using UnityEngine;

public class UiInstaller : ValidatedMonoBehaviour, IInstaller
{
    [SerializeField, Scene]
    private TrashBin _trashBin;
    
    [SerializeField, Scene]
    private PopupTextService _popupTextService;

    [SerializeField]
    private TooltipService _tooltipService;
    
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        var tooltipSettings = Resources.Load<TooltipSettings>("Settings/UI/TooltipSettings") ?? TooltipSettings.CreateFallback();
        var tooltipService = _tooltipService != null ? _tooltipService : CreateRuntimeTooltipService();
        tooltipService.Configure(tooltipSettings);

        containerBuilder
            .AddScoped(_ => _trashBin, typeof(TrashBin))
            .AddScoped(_ => _popupTextService, typeof(IPopupTextService))
            .AddSingleton(tooltipSettings)
            .AddScoped(_ => tooltipService, typeof(TooltipService), typeof(ITooltipService));
    }

    private TooltipService CreateRuntimeTooltipService()
    {
        var tooltipServiceObject = new GameObject("TooltipService");
        tooltipServiceObject.transform.SetParent(transform, false);
        _tooltipService = tooltipServiceObject.AddComponent<TooltipService>();
        return _tooltipService;
    }
}
