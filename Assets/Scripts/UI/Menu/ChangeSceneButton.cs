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
    private int _sceneIndex;

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

        if (_sceneIndex == 1)
            GameplayTutorialOptions.SetShouldRunTutorial(true);

        await _sceneTransitionService.TryLoadSceneAsync(_sceneIndex);
    }
}
