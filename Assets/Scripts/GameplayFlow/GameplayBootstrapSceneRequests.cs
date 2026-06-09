using System;
using Cysharp.Threading.Tasks;
using Reflex.Core;
using Reflex.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

internal static class GameplayBootstrapSceneRequests
{
    private enum RequestType
    {
        None,
        Downtime,
        LevelGameplay,
        ArcadeLevel
    }

    private static RequestType _requestType;
    private static int _bootstrapBuildIndex = -1;
    private static Level _level;
    private static Dough[] _doughs = Array.Empty<Dough>();
    private static Filling[] _fillings = Array.Empty<Filling>();
    private static bool _isProcessing;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        Clear();
    }

    public static void RequestDowntime(int bootstrapBuildIndex)
    {
        _bootstrapBuildIndex = bootstrapBuildIndex;
        _requestType = RequestType.Downtime;
        _level = null;
        _doughs = Array.Empty<Dough>();
        _fillings = Array.Empty<Filling>();
    }

    public static void RequestLevelGameplay(int bootstrapBuildIndex, Level level, Dough[] doughs, Filling[] fillings)
    {
        _bootstrapBuildIndex = bootstrapBuildIndex;
        _requestType = RequestType.LevelGameplay;
        _level = level;
        _doughs = doughs ?? Array.Empty<Dough>();
        _fillings = fillings ?? Array.Empty<Filling>();
    }

    public static void RequestArcadeLevel(int bootstrapBuildIndex, Level level)
    {
        _bootstrapBuildIndex = bootstrapBuildIndex;
        _requestType = RequestType.ArcadeLevel;
        _level = level;
        _doughs = Array.Empty<Dough>();
        _fillings = Array.Empty<Filling>();
    }

    public static bool HasPendingRequestForBootstrap(int bootstrapBuildIndex)
    {
        return _requestType != RequestType.None && _bootstrapBuildIndex == bootstrapBuildIndex;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode _)
    {
        if (_requestType == RequestType.None || _isProcessing)
            return;

        if (scene.buildIndex != _bootstrapBuildIndex)
            return;

        ProcessRequestAsync(scene).Forget();
    }

    private static async UniTaskVoid ProcessRequestAsync(Scene bootstrapScene)
    {
        _isProcessing = true;

        try
        {
            await UniTask.Yield(PlayerLoopTiming.Update);

            if (!bootstrapScene.IsValid() || !bootstrapScene.isLoaded)
                return;

            var container = bootstrapScene.GetSceneContainer();

            switch (_requestType)
            {
                case RequestType.Downtime:
                    await container.Resolve<GameplayLoopFlowController>().LoadDowntime();
                    break;
                case RequestType.LevelGameplay:
                    await container.Resolve<LevelSelector>().StartConfiguredLevel(_level, _doughs, _fillings);
                    break;
                case RequestType.ArcadeLevel:
                    await container.Resolve<LevelSelector>().PlayArcadeLevel(_level);
                    break;
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to process pending gameplay bootstrap scene request.\n{exception}");
        }
        finally
        {
            Clear();
            _isProcessing = false;
        }
    }

    private static void Clear()
    {
        _requestType = RequestType.None;
        _bootstrapBuildIndex = -1;
        _level = null;
        _doughs = Array.Empty<Dough>();
        _fillings = Array.Empty<Filling>();
    }
}

internal static class GameplayBootstrapGuardUtility
{
    public static void PreventSceneStartup(GameObject guardObject, MonoBehaviour keepAlive, SceneScope sceneScope)
    {
        if (guardObject == null)
            return;

        if (sceneScope != null)
            sceneScope.enabled = false;

        var components = guardObject.GetComponents<MonoBehaviour>();
        for (var i = 0; i < components.Length; i++)
        {
            var component = components[i];
            if (component == null || component == keepAlive)
                continue;

            component.enabled = false;
        }

        var scene = guardObject.scene;
        if (!scene.IsValid())
            return;

        var roots = scene.GetRootGameObjects();
        for (var i = 0; i < roots.Length; i++)
        {
            var root = roots[i];
            if (root != null && root != guardObject)
                root.SetActive(false);
        }
    }
}
