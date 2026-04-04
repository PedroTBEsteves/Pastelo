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

    [Header("Mouse Parallax")]
    [SerializeField]
    private Vector2 _mouseParallaxIntensity = new(0.5f, 0.35f);

    [SerializeField, Min(0f)]
    private float _mouseParallaxSmoothTime = 0.12f;
    
    private CameraController _cameraController;
    private MouseParallaxService _mouseParallaxService;
    
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        _cameraController = new CameraController(_sectionPositions, _transitionDuration, _transitionEase, _sectionController);
        _mouseParallaxService = new MouseParallaxService(_mouseParallaxIntensity, _mouseParallaxSmoothTime);
        
        containerBuilder
            .AddScoped(_ => _cameraController, typeof(CameraController))
            .AddScoped(_ => _sectionController)
            .AddScoped(_ => _mouseParallaxService, typeof(MouseParallaxService), typeof(ITickable));
    }

    private void Awake()
    {
        _cameraController.GoImmediatelyToSection(CameraSection.Balcony);
    }
}
