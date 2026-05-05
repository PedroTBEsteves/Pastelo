using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

public class ChangeSceneButton : ValidatedMonoBehaviour
{
    [Inject]
    private readonly ISceneTransitionService _sceneTransitionService;

    [SerializeField, Self]
    private Button _button;
    
    [SerializeField]
    private SceneReference _sceneReference;
    
    [SerializeField]
    private bool _shouldRunTutorial;

    private void Awake()
    {
        _button.onClick.AddListener(OnButtonClicked);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (_sceneTransitionService.IsTransitioning)
            return;

        GameplayTutorialOptions.SetShouldRunTutorial(_shouldRunTutorial);

        _sceneTransitionService.TryLoadSceneAsync(_sceneReference).Forget();
    }
}
