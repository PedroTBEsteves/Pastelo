using AYellowpaper.SerializedCollections;
using PrimeTween;
using Reflex.Core;
using UnityEngine;

public class CameraInstaller : MonoBehaviour, IInstaller
{
    [SerializeField]
    private SerializedDictionary<CameraSection, Vector3> _sectionPositions;
    
    [SerializeField]
    private TweenSettings _cameraTransitionSettings;
    
    [SerializeField]
    private AudioSource _transitionAudioSource;
    
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        var cameraController = new CameraController(_sectionPositions, _cameraTransitionSettings, _transitionAudioSource);
        
        containerBuilder.AddScoped(_ => cameraController, typeof(CameraController));
    }
}
