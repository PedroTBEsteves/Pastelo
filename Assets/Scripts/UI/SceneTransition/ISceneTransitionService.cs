using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using UnityEngine.SceneManagement;

public interface ISceneTransitionService
{
    bool IsTransitioning { get; }
    UniTask<bool> TryLoadSceneAsync(
        SceneReference sceneReference,
        LoadSceneMode loadSceneMode = LoadSceneMode.Single,
        bool useFadeOut = true,
        bool useFadeIn = true,
        CancellationToken cancellationToken = default);

    UniTask<bool> TryRunTransitionAsync(
        Func<CancellationToken, UniTask<bool>> transitionOperation,
        bool useFadeOut = true,
        bool useFadeIn = true,
        CancellationToken cancellationToken = default);
}
