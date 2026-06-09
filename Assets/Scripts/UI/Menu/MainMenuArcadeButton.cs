using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public sealed class MainMenuArcadeButton : ValidatedMonoBehaviour
{
    [Inject]
    private readonly ISceneTransitionService _sceneTransitionService;

    [SerializeField, Self]
    private Button _button;

    [SerializeField]
    private SceneReference _gameplayBootstrapScene;

    [SerializeField]
    private Level _arcadeLevel;

    [SerializeField]
    private bool _runTutorial = true;

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

        if (!TryValidateConfiguration(out var bootstrapBuildIndex))
            return;

        GameplayTutorialOptions.SetShouldRunTutorial(_runTutorial);
        GameplayBootstrapSceneRequests.RequestArcadeLevel(bootstrapBuildIndex, _arcadeLevel);
        _sceneTransitionService.TryLoadSceneAsync(_gameplayBootstrapScene).Forget();
    }

    private bool TryValidateConfiguration(out int bootstrapBuildIndex)
    {
        bootstrapBuildIndex = -1;

        if (_arcadeLevel == null)
        {
            Debug.LogError($"{nameof(MainMenuArcadeButton)} on '{name}' requires an arcade {nameof(Level)}.", this);
            return false;
        }

        if (_gameplayBootstrapScene == null)
        {
            Debug.LogError($"{nameof(MainMenuArcadeButton)} on '{name}' requires a Gameplay Bootstrap scene reference.", this);
            return false;
        }

        if (!_gameplayBootstrapScene.TryGetBuildIndex(out bootstrapBuildIndex) || bootstrapBuildIndex < 0)
        {
            Debug.LogError($"{nameof(MainMenuArcadeButton)} on '{name}' could not resolve the Gameplay Bootstrap build index.", this);
            return false;
        }

        return true;
    }
}
