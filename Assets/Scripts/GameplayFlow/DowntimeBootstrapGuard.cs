using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using Reflex.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(SceneScope.ExecutionOrder - 1000)]
public sealed class DowntimeBootstrapGuard : MonoBehaviour
{
    [SerializeField]
    private SceneReference _gameplayBootstrapScene;

    [SerializeField]
    private SceneScope _sceneScope;

    private void Awake()
    {
        if (!TryResolveBootstrapBuildIndex(out var bootstrapBuildIndex))
            return;

        if (IsSceneLoaded(bootstrapBuildIndex))
            return;

        GameplayBootstrapGuardUtility.PreventSceneStartup(gameObject, this, _sceneScope);
        GameplayBootstrapSceneRequests.RequestDowntime(bootstrapBuildIndex);
        LoadBootstrapAsync(bootstrapBuildIndex).Forget();
    }

    private async UniTaskVoid LoadBootstrapAsync(int bootstrapBuildIndex)
    {
        var loadOperation = SceneManager.LoadSceneAsync(bootstrapBuildIndex, LoadSceneMode.Single);
        if (loadOperation == null)
        {
            Debug.LogError($"{nameof(DowntimeBootstrapGuard)} failed to start loading Gameplay Bootstrap.", this);
            return;
        }

        await loadOperation.ToUniTask();
    }

    private bool TryResolveBootstrapBuildIndex(out int bootstrapBuildIndex)
    {
        bootstrapBuildIndex = -1;

        if (_gameplayBootstrapScene == null)
        {
            Debug.LogError($"{nameof(DowntimeBootstrapGuard)} requires a Gameplay Bootstrap scene reference.", this);
            return false;
        }

        if (!_gameplayBootstrapScene.TryGetBuildIndex(out bootstrapBuildIndex) || bootstrapBuildIndex < 0)
        {
            Debug.LogError($"{nameof(DowntimeBootstrapGuard)} could not resolve the Gameplay Bootstrap build index.", this);
            return false;
        }

        return true;
    }

    private static bool IsSceneLoaded(int buildIndex)
    {
        for (var i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.buildIndex == buildIndex && scene.isLoaded)
                return true;
        }

        return false;
    }
}
