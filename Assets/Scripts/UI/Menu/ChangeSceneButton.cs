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

    private void Awake()
    {
        _button.onClick.AddListener(OnButtonClicked);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnButtonClicked);
    }

    private async void OnButtonClicked()
    {
        if (_sceneTransitionService.IsTransitioning)
            return;

        if (IsTutorialScene())
            GameplayTutorialOptions.SetShouldRunTutorial(true);

        await _sceneTransitionService.TryLoadSceneAsync(_sceneReference);
    }

    private bool IsTutorialScene()
    {
        return _sceneReference != null
               && _sceneReference.TryGetBuildIndex(out var sceneBuildIndex)
               && sceneBuildIndex == 1;
    }
}
