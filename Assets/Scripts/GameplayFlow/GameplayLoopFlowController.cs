using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using Reflex.Core;
using Reflex.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameplayLoopFlowController : MonoBehaviour
{
    [SerializeField]
    private SceneReference _levelGameplayScene;

    [SerializeField]
    private SceneReference _downtimeScene;

    private Scene _currentLoadedScene;
    private int _targetSceneBuildIndex = -1;
    private Container _parentContainer;
    private bool _isLoading;

    private void Awake()
    {
        TryResolveParentContainer(out _parentContainer);
    }

    private async UniTaskVoid Start()
    {
        await LoadDowntime();
    }

    public UniTask<bool> LoadLevelGameplay()
    {
        return LoadSceneAsync(_levelGameplayScene);
    }

    public UniTask<bool> LoadDowntime()
    {
        return LoadSceneAsync(_downtimeScene);
    }

    private async UniTask<bool> LoadSceneAsync(SceneReference sceneReference)
    {
        if (!TryResolveTargetBuildIndex(sceneReference, out var sceneBuildIndex))
            return false;

        if (TryActivateCurrentScene(sceneBuildIndex))
            return true;

        if (_isLoading)
        {
            Debug.LogWarning($"{nameof(GameplayLoopFlowController)} is already loading a scene.", this);
            return false;
        }

        if (!TryResolveParentContainer(out var parentContainer))
            return false;

        var cancellationToken = this.GetCancellationTokenOnDestroy();
        _isLoading = true;

        try
        {
            if (!await UnloadCurrentLoadedSceneIfNeededAsync(sceneBuildIndex, cancellationToken))
                return false;

            _parentContainer = parentContainer;
            _targetSceneBuildIndex = sceneBuildIndex;
            SceneScope.OnSceneContainerBuilding += OverrideParent;

            var loadOperation = SceneManager.LoadSceneAsync(sceneBuildIndex, LoadSceneMode.Additive);
            if (loadOperation == null)
            {
                Debug.LogError($"Failed to start loading scene with build index '{sceneBuildIndex}'.", this);
                return false;
            }

            await loadOperation;
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
