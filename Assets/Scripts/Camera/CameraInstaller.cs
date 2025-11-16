using AYellowpaper.SerializedCollections;
using KBCore.Refs;
using Reflex.Core;
using UnityEngine;

public class CameraInstaller : ValidatedMonoBehaviour, IInstaller
{
    [SerializeField]
    private SerializedDictionary<CameraSection, Vector3> _sectionPositions;

    [SerializeField]
    private float _cameraSpeed;
    
    [SerializeField]
    private float _cameraAcceleration = 30f;
    
    [SerializeField, Scene]
    private SectionController _sectionController;
    
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        var cameraController = new CameraController(_sectionPositions, _cameraSpeed, _cameraAcceleration);
        
        containerBuilder
            .AddScoped(_ => cameraController, typeof(CameraController), typeof(ITickable))
            .AddScoped(_ => _sectionController);
    }
}
