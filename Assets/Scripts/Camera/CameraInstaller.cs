using AYellowpaper.SerializedCollections;
using KBCore.Refs;
using PrimeTween;
using Reflex.Core;
using UnityEngine;

public class CameraInstaller : ValidatedMonoBehaviour, IInstaller
{
    [SerializeField]
    private SerializedDictionary<CameraSection, Vector3> _sectionPositions;

    [SerializeField]
    private float _transitionDuration = 0.5f;

    [SerializeField]
    private Ease _transitionEase = Ease.InOutSine;
    
    [SerializeField, Scene]
    private SectionController _sectionController;
    
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        var cameraController = new CameraController(_sectionPositions, _transitionDuration, _transitionEase, _sectionController);
        
        containerBuilder
            .AddScoped(_ => cameraController, typeof(CameraController))
            .AddScoped(_ => _sectionController);
    }
}
