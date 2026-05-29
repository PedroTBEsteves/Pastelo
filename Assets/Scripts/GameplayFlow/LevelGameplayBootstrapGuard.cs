using System;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using Reflex.Core;
using Reflex.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(SceneScope.ExecutionOrder - 1000)]
public sealed class LevelGameplayBootstrapGuard : MonoBehaviour
{
    [SerializeField]
    private SceneReference _gameplayBootstrapScene;

    [SerializeField]
    private SceneScope _sceneScope;

    [SerializeField]
    private Level _level;

    [SerializeField]
    private Dough[] _doughs = Array.Empty<Dough>();

    [SerializeField]
    private Filling[] _fillings = Array.Empty<Filling>();

    private void Awake()
    {
        if (!TryResolveBootstrapBuildIndex(out var bootstrapBuildIndex))
            return;

        if (!TryValidateConfiguredLevel())
            return;

        if (!TryGetLoadedBootstrapScene(bootstrapBuildIndex, out var bootstrapScene))
        {
            RequestLevelAndReloadBootstrap(bootstrapBuildIndex);
            return;
        }

        if (TryResolveLevelSelector(bootstrapScene, out var levelSelector) && levelSelector.SelectedLevel != null)
            return;

        RequestLevelAndReloadBootstrap(bootstrapBuildIndex);
    }

    private void RequestLevelAndReloadBootstrap(int bootstrapBuildIndex)
    {
        GameplayBootstrapGuardUtility.PreventSceneStartup(gameObject, this, _sceneScope);
        GameplayBootstrapSceneRequests.RequestLevelGameplay(bootstrapBuildIndex, _level, _doughs, _fillings);
        LoadBootstrapAsync(bootstrapBuildIndex).Forget();
    }

    private async UniTaskVoid LoadBootstrapAsync(int bootstrapBuildIndex)
    {
        var loadOperation = SceneManager.LoadSceneAsync(bootstrapBuildIndex, LoadSceneMode.Single);
        if (loadOperation == null)
        {
            Debug.LogError($"{nameof(LevelGameplayBootstrapGuard)} failed to start loading Gameplay Bootstrap.", this);
            return;
        }

        await loadOperation.ToUniTask();
    }

    private bool TryResolveBootstrapBuildIndex(out int bootstrapBuildIndex)
    {
        bootstrapBuildIndex = -1;

        if (_gameplayBootstrapScene == null)
        {
            Debug.LogError($"{nameof(LevelGameplayBootstrapGuard)} requires a Gameplay Bootstrap scene reference.", this);
            return false;
        }

        if (!_gameplayBootstrapScene.TryGetBuildIndex(out bootstrapBuildIndex) || bootstrapBuildIndex < 0)
        {
            Debug.LogError($"{nameof(LevelGameplayBootstrapGuard)} could not resolve the Gameplay Bootstrap build index.", this);
            return false;
        }

        return true;
    }

    private bool TryValidateConfiguredLevel()
    {
        if (_level == null)
        {
            Debug.LogError($"{nameof(LevelGameplayBootstrapGuard)} requires a configured {nameof(Level)}.", this);
            return false;
        }

        return true;
    }

    private static bool TryGetLoadedBootstrapScene(int buildIndex, out Scene bootstrapScene)
    {
        for (var i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.buildIndex == buildIndex && scene.isLoaded)
            {
                bootstrapScene = scene;
                return true;
            }
        }

        bootstrapScene = default;
        return false;
    }

    private static bool TryResolveLevelSelector(Scene bootstrapScene, out LevelSelector levelSelector)
    {
        levelSelector = null;

        try
        {
            levelSelector = bootstrapScene.GetSceneContainer().Resolve<LevelSelector>();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
