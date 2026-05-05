using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using PrimeTween;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionService : MonoBehaviour, ISceneTransitionService
{
    [SerializeField]
    private Canvas _canvas;

    [SerializeField]
    private CanvasGroup _overlayCanvasGroup;

    [SerializeField]
    private AudioMixer _masterMixer;

    [SerializeField]
    private string _masterVolumeParameter = "MasterVolume";

    [SerializeField]
    private float _fadedVolumeDb = -80f;

    [SerializeField]
    private TweenSettings<float> _fadeOutTweenSettings = new(1f, 0.35f, Ease.OutQuad, useUnscaledTime: true);

    [SerializeField]
    private TweenSettings<float> _fadeInTweenSettings = new(0f, 0.35f, Ease.OutQuad, useUnscaledTime: true);

    private Tween _fadeTween;
    private bool _isTransitioning;
    private float _audioFadeFromDb;
    private float _audioFadeToDb;

    public bool IsTransitioning => _isTransitioning;

    private void Awake()
    {
        PrepareOverlay();
    }

    private void OnDestroy()
    {
        StopFadeTween();
    }

    public UniTask<bool> TryLoadSceneAsync(
        SceneReference sceneReference,
        LoadSceneMode loadSceneMode = LoadSceneMode.Single,
        bool useFadeOut = true,
        bool useFadeIn = true,
        CancellationToken cancellationToken = default)
    {
        if (!TryResolveTargetBuildIndex(sceneReference, out var sceneBuildIndex))
            return UniTask.FromResult(false);

        return TryRunTransitionAsync(
            async transitionCancellationToken =>
            {
                var loadOperation = SceneManager.LoadSceneAsync(sceneBuildIndex, loadSceneMode);
                if (loadOperation == null)
                {
                    Debug.LogError($"Failed to start loading scene with build index '{sceneBuildIndex}'.", this);
                    return false;
                }

                await loadOperation.ToUniTask(cancellationToken: transitionCancellationToken);
                await UniTask.Yield(PlayerLoopTiming.Update, transitionCancellationToken);
                return true;
            },
            useFadeOut,
            useFadeIn,
            cancellationToken);
    }

    public async UniTask<bool> TryRunTransitionAsync(
        Func<CancellationToken, UniTask<bool>> transitionOperation,
        bool useFadeOut = true,
        bool useFadeIn = true,
        CancellationToken cancellationToken = default)
    {
        if (IsTransitioning)
            return false;

        if (transitionOperation == null)
        {
            Debug.LogError($"{nameof(SceneTransitionService)} requires a transition operation.", this);
            return false;
        }

        var usesFade = useFadeOut || useFadeIn;
        if (!TryValidateSetup(usesFade, out var startingVolumeDb))
            return false;

        _isTransitioning = true;

        try
        {
            SetOverlayBlocking(usesFade);

            if (useFadeOut)
                await PlayFadeAsync(_fadeOutTweenSettings, startingVolumeDb, _fadedVolumeDb);

            cancellationToken.ThrowIfCancellationRequested();
            RestoreScaledTime();

            var completed = await transitionOperation(cancellationToken);
            if (!completed)
                return false;

            if (useFadeIn)
            {
                _overlayCanvasGroup.alpha = 1f;
                await PlayFadeAsync(_fadeInTweenSettings, _fadedVolumeDb, startingVolumeDb);
            }

            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        finally
        {
            if (usesFade)
                _masterMixer.SetFloat(_masterVolumeParameter, startingVolumeDb);

            ResetOverlayState();
            _isTransitioning = false;
        }
    }

    private async UniTask PlayFadeAsync(TweenSettings<float> visualSettings, float audioFromDb, float audioToDb)
    {
        StopFadeTween();

        _audioFadeFromDb = audioFromDb;
        _audioFadeToDb = audioToDb;
        _masterMixer.SetFloat(_masterVolumeParameter, _audioFadeFromDb);

        _fadeTween = Tween.Alpha(_overlayCanvasGroup, visualSettings)
            .OnUpdate(this, static (service, tween) => service.UpdateAudioFade(tween.interpolationFactor));

        await _fadeTween;

        _masterMixer.SetFloat(_masterVolumeParameter, _audioFadeToDb);
    }

    private void UpdateAudioFade(float interpolationFactor)
    {
        float volumeDb = Mathf.Lerp(_audioFadeFromDb, _audioFadeToDb, interpolationFactor);
        _masterMixer.SetFloat(_masterVolumeParameter, volumeDb);
    }

    private bool TryValidateSetup(bool usesFade, out float startingVolumeDb)
    {
        startingVolumeDb = 0f;

        if (_canvas == null)
        {
            Debug.LogError($"{nameof(SceneTransitionService)} requires a {nameof(Canvas)} reference.", this);
            return false;
        }

        if (_overlayCanvasGroup == null)
        {
            Debug.LogError($"{nameof(SceneTransitionService)} requires an overlay {nameof(CanvasGroup)} reference.", this);
            return false;
        }

        if (!usesFade)
            return true;

        if (_masterMixer == null)
        {
            Debug.LogError($"{nameof(SceneTransitionService)} requires a master mixer reference.", this);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_masterVolumeParameter))
        {
            Debug.LogError($"{nameof(SceneTransitionService)} requires a mixer parameter name.", this);
            return false;
        }

        if (!_masterMixer.GetFloat(_masterVolumeParameter, out startingVolumeDb))
        {
            Debug.LogError(
                $"{nameof(SceneTransitionService)} could not read mixer parameter '{_masterVolumeParameter}'.",
                this);
            return false;
        }

        return true;
    }

    private bool TryResolveTargetBuildIndex(SceneReference sceneReference, out int sceneBuildIndex)
    {
        sceneBuildIndex = -1;

        if (sceneReference == null)
        {
            Debug.LogError($"{nameof(SceneTransitionService)} requires a valid {nameof(SceneReference)}.", this);
            return false;
        }

        if (!sceneReference.TryGetBuildIndex(out sceneBuildIndex) || sceneBuildIndex < 0)
        {
            Debug.LogError($"{nameof(SceneTransitionService)} could not resolve the target scene build index.", this);
            return false;
        }

        return true;
    }

    private void PrepareOverlay()
    {
        _canvas.enabled = true;

        if (_overlayCanvasGroup != null)
        {
            _overlayCanvasGroup.alpha = 0f;
            _overlayCanvasGroup.interactable = false;
            _overlayCanvasGroup.blocksRaycasts = false;
        }
    }

    private void SetOverlayBlocking(bool isBlocking)
    {
        if (_overlayCanvasGroup == null)
            return;

        _overlayCanvasGroup.interactable = false;
        _overlayCanvasGroup.blocksRaycasts = isBlocking;
    }

    private void ResetOverlayState()
    {
        if (_overlayCanvasGroup == null)
            return;

        _overlayCanvasGroup.alpha = 0f;
        _overlayCanvasGroup.interactable = false;
        _overlayCanvasGroup.blocksRaycasts = false;
    }

    private static void RestoreScaledTime()
    {
        if (!Mathf.Approximately(Time.timeScale, 1f))
            Time.timeScale = 1f;
    }

    private void StopFadeTween()
    {
        if (_fadeTween.isAlive)
            _fadeTween.Stop();
    }
}
