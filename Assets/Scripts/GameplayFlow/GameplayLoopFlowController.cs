using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using Reflex.Attributes;
using Reflex.Core;
using Reflex.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameplayLoopFlowController : MonoBehaviour
{
    [Inject]
    private readonly ISceneTransitionService _sceneTransitionService;

    [SerializeField]
    private SceneReference _levelGameplayScene;

    [SerializeField]
    private SceneReference _downtimeScene;

    [Header("Tutorial Autostart")]
    [SerializeField]
    private Level _tutorialLevel;

    [SerializeField]
    private Dough[] _tutorialDoughs = Array.Empty<Dough>();

    [SerializeField]
    private Filling[] _tutorialFillings = Array.Empty<Filling>();

    private Scene _currentLoadedScene;
    private int _targetSceneBuildIndex = -1;
    private Container _parentContainer;
    private bool _isLoading;

    private void Awake()
    {
        TryResolveParentContainer(out _parentContainer);
    }

    private void Start()
    {
        LoadInitialLoopSceneAsync().Forget();
    }

    public UniTask<bool> LoadLevelGameplay()
    {
        return LoadSceneAsync(_levelGameplayScene);
    }

    public UniTask<bool> LoadDowntime()
    {
        return LoadSceneAsync(_downtimeScene);
    }

    private async UniTaskVoid LoadInitialLoopSceneAsync()
    {
        try
        {
            await UniTask.Yield(PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());

            if (GameplayBootstrapSceneRequests.HasPendingRequestForBootstrap(gameObject.scene.buildIndex))
                return;

            if (!GameplayTutorialOptions.PeekShouldRunTutorial())
            {
                await LoadDowntime();
                return;
            }

            if (!TryValidateTutorialAutostartSettings())
                return;

            if (!TryResolveParentContainer(out var parentContainer))
                return;

            _parentContainer = parentContainer;

            LevelSelector levelSelector;
            try
            {
                levelSelector = _parentContainer.Resolve<LevelSelector>();
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"{nameof(GameplayLoopFlowController)} could not resolve {nameof(LevelSelector)} for tutorial autostart.\n{exception}",
                    this);
                return;
            }

            try
            {
                await levelSelector.StartConfiguredLevel(_tutorialLevel, _tutorialDoughs, _tutorialFillings);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"{nameof(GameplayLoopFlowController)} failed to start tutorial level '{_tutorialLevel.name}'.\n{exception}",
                    this);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async UniTask<bool> LoadSceneAsync(SceneReference sceneReference)
    {
        if (!TryResolveTargetBuildIndex(sceneReference, out var sceneBuildIndex))
            return false;

        if (TryActivateCurrentScene(sceneBuildIndex))
            return true;

        if (_isLoading)
            return await WaitForCurrentLoadAndRetryAsync(sceneReference, sceneBuildIndex);

        if (!TryResolveParentContainer(out var parentContainer))
            return false;

        var cancellationToken = this.GetCancellationTokenOnDestroy();
        _isLoading = true;

        try
        {
            await WaitForSceneTransitionServiceAsync(cancellationToken);

            _parentContainer = parentContainer;
            _targetSceneBuildIndex = sceneBuildIndex;
            SceneScope.OnSceneContainerBuilding += OverrideParent;

            return await _sceneTransitionService.TryRunTransitionAsync(
                transitionCancellationToken => LoadLoopSceneAsync(sceneBuildIndex, transitionCancellationToken),
                useFadeOut: true,
                useFadeIn: true,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        finally
        {
            SceneScope.OnSceneContainerBuilding -= OverrideParent;
            _targetSceneBuildIndex = -1;
            _isLoading = false;
        }
    }

    private async UniTask<bool> WaitForCurrentLoadAndRetryAsync(SceneReference sceneReference, int sceneBuildIndex)
    {
        var cancellationToken = this.GetCancellationTokenOnDestroy();

        while (_isLoading)
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

        if (TryActivateCurrentScene(sceneBuildIndex))
            return true;

        return await LoadSceneAsync(sceneReference);
    }

    private async UniTask WaitForSceneTransitionServiceAsync(CancellationToken cancellationToken)
    {
        while (_sceneTransitionService.IsTransitioning)
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
    }

    private async UniTask<bool> LoadLoopSceneAsync(
        int sceneBuildIndex,
        CancellationToken cancellationToken)
    {
        if (!await UnloadCurrentLoadedSceneIfNeededAsync(sceneBuildIndex, cancellationToken))
            return false;

        var loadOperation = SceneManager.LoadSceneAsync(sceneBuildIndex, LoadSceneMode.Additive);
        if (loadOperation == null)
        {
            Debug.LogError($"Failed to start loading scene with build index '{sceneBuildIndex}'.", this);
            return false;
        }

        await loadOperation.ToUniTask(cancellationToken: cancellationToken);
        await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

        if (!TryGetLoadedSceneByBuildIndex(sceneBuildIndex, out var loadedScene))
        {
            Debug.LogError($"Failed to load scene with build index '{sceneBuildIndex}'.", this);
            return false;
        }

        if (!loadedScene.IsValid())
        {
            Debug.LogError($"Loaded scene with build index '{sceneBuildIndex}' is invalid.", this);
            return false;
        }

        if (!loadedScene.isLoaded)
        {
            Debug.LogError($"Loaded scene with build index '{sceneBuildIndex}' is not marked as loaded.", this);
            return false;
        }

        if (!SceneManager.SetActiveScene(loadedScene))
        {
            Debug.LogError($"Failed to set active scene '{loadedScene.name}'.", this);
            return false;
        }

        _currentLoadedScene = loadedScene;
        return true;
    }

    private void OverrideParent(Scene scene, ContainerBuilder builder)
    {
        if (scene.buildIndex != _targetSceneBuildIndex)
            return;

        builder.SetParent(_parentContainer);
    }

    private bool TryResolveTargetBuildIndex(SceneReference sceneReference, out int sceneBuildIndex)
    {
        sceneBuildIndex = -1;

        if (sceneReference == null)
        {
            Debug.LogError($"{nameof(GameplayLoopFlowController)} requires a valid {nameof(SceneReference)}.", this);
            return false;
        }

        if (!sceneReference.TryGetBuildIndex(out sceneBuildIndex) || sceneBuildIndex < 0)
        {
            Debug.LogError($"{nameof(GameplayLoopFlowController)} could not resolve the target scene build index.", this);
            return false;
        }

        return true;
    }

    private bool TryValidateTutorialAutostartSettings()
    {
        if (_tutorialLevel == null)
        {
            Debug.LogError($"{nameof(GameplayLoopFlowController)} requires a configured tutorial {nameof(Level)}.", this);
            return false;
        }

        if (!HasConfiguredEntry(_tutorialDoughs))
        {
            Debug.LogError($"{nameof(GameplayLoopFlowController)} requires at least one configured tutorial {nameof(Dough)}.", this);
            return false;
        }

        if (!HasConfiguredEntry(_tutorialFillings))
        {
            Debug.LogError($"{nameof(GameplayLoopFlowController)} requires at least one configured tutorial {nameof(Filling)}.", this);
            return false;
        }

        return true;
    }

    private static bool HasConfiguredEntry<T>(T[] entries) where T : UnityEngine.Object
    {
        if (entries == null)
            return false;

        for (var i = 0; i < entries.Length; i++)
        {
            if (entries[i] != null)
                return true;
        }

        return false;
    }

    private bool TryResolveParentContainer(out Container parentContainer)
    {
        parentContainer = null;

        try
        {
            parentContainer = gameObject.scene.GetSceneContainer();
            return true;
        }
        catch (System.Exception exception)
        {
            Debug.LogError(
                $"{nameof(GameplayLoopFlowController)} could not resolve the parent container for scene '{gameObject.scene.name}'.\n{exception}",
                this);
            return false;
        }
    }

    private async UniTask<bool> UnloadCurrentLoadedSceneIfNeededAsync(int nextSceneBuildIndex, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentLoadedScene(out var currentLoadedScene))
            return true;

        if (currentLoadedScene.buildIndex == nextSceneBuildIndex)
            return true;

        var unloadOperation = SceneManager.UnloadSceneAsync(currentLoadedScene);
        if (unloadOperation == null)
        {
            Debug.LogError($"Failed to start unloading scene '{currentLoadedScene.name}'.", this);
            return false;
        }

        await unloadOperation.ToUniTask(cancellationToken: cancellationToken);
        await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

        if (TryGetLoadedSceneByBuildIndex(currentLoadedScene.buildIndex, out _))
        {
            Debug.LogError($"Scene '{currentLoadedScene.name}' is still loaded after unload completed.", this);
            return false;
        }

        _currentLoadedScene = default;
        return true;
    }

    private bool TryActivateCurrentScene(int sceneBuildIndex)
    {
        if (!TryGetCurrentLoadedScene(out var currentLoadedScene))
            return false;

        if (currentLoadedScene.buildIndex != sceneBuildIndex)
            return false;

        if (!SceneManager.SetActiveScene(currentLoadedScene))
        {
            Debug.LogError($"Failed to set active scene '{currentLoadedScene.name}'.", this);
            return false;
        }

        return true;
    }

    private bool TryGetCurrentLoadedScene(out Scene currentLoadedScene)
    {
        currentLoadedScene = _currentLoadedScene;

        if (!currentLoadedScene.IsValid() || !currentLoadedScene.isLoaded)
        {
            _currentLoadedScene = default;
            currentLoadedScene = default;
            return false;
        }

        return true;
    }

    private bool TryGetLoadedSceneByBuildIndex(int sceneBuildIndex, out Scene loadedScene)
    {
        for (var i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.buildIndex == sceneBuildIndex)
            {
                loadedScene = scene;
                return true;
            }
        }

        loadedScene = default;
        return false;
    }
}
