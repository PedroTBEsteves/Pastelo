using System.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionService : MonoBehaviour, ISceneTransitionService
{
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

    private Canvas _canvas;
    private CanvasGroup _overlayCanvasGroup;
    private Image _overlayImage;
    private bool _isTransitioning;
    private float _audioFadeFromDb;
    private float _audioFadeToDb;

    public bool IsTransitioning => _isTransitioning;

    private void Awake()
    {
        ResolveReferences();
        PrepareOverlay();
    }

    public async Task<bool> TryLoadSceneAsync(int sceneIndex)
    {
        if (IsTransitioning)
            return false;

        if (!TryValidateSetup(out float startingVolumeDb))
            return false;

        _isTransitioning = true;

        try
        {
            SetOverlayBlocking(true);
            await PlayFadeAsync(_fadeOutTweenSettings, startingVolumeDb, _fadedVolumeDb);

            var loadOperation = SceneManager.LoadSceneAsync(sceneIndex);
            await AwaitAsyncOperation(loadOperation);
            await Task.Yield();

            _overlayCanvasGroup.alpha = 1f;
            await PlayFadeAsync(_fadeInTweenSettings, _fadedVolumeDb, startingVolumeDb);
            SetOverlayBlocking(false);
            return true;
        }
        finally
        {
            _isTransitioning = false;
        }
    }

    private async Task PlayFadeAsync(TweenSettings<float> visualSettings, float audioFromDb, float audioToDb)
    {
        _audioFadeFromDb = audioFromDb;
        _audioFadeToDb = audioToDb;
        _masterMixer.SetFloat(_masterVolumeParameter, _audioFadeFromDb);

        var tween = Tween.Alpha(_overlayCanvasGroup, visualSettings)
            .OnUpdate(this, static (service, tween) => service.UpdateAudioFade(tween.interpolationFactor));

        await tween;

        _masterMixer.SetFloat(_masterVolumeParameter, _audioFadeToDb);
    }

    private void UpdateAudioFade(float interpolationFactor)
    {
        float volumeDb = Mathf.Lerp(_audioFadeFromDb, _audioFadeToDb, interpolationFactor);
        _masterMixer.SetFloat(_masterVolumeParameter, volumeDb);
    }

    private bool TryValidateSetup(out float startingVolumeDb)
    {
        startingVolumeDb = 0f;

        ResolveReferences();

        if (_canvas == null)
        {
            Debug.LogError($"{nameof(SceneTransitionService)} requires a {nameof(Canvas)} on the service prefab.", this);
            return false;
        }

        if (_overlayImage == null)
        {
            Debug.LogError($"{nameof(SceneTransitionService)} requires an overlay {nameof(Image)} in the service prefab.", this);
            return false;
        }

        if (_overlayCanvasGroup == null)
        {
            Debug.LogError($"{nameof(SceneTransitionService)} requires a {nameof(CanvasGroup)} on the overlay object.", this);
            return false;
        }

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

    private void ResolveReferences()
    {
        if (_canvas == null)
            _canvas = GetComponent<Canvas>();

        if (_overlayImage == null)
            _overlayImage = GetComponentInChildren<Image>(true);

        if (_overlayCanvasGroup == null)
            _overlayCanvasGroup = GetComponentInChildren<CanvasGroup>(true);
    }

    private void PrepareOverlay()
    {
        if (_canvas != null)
            _canvas.enabled = true;

        if (_overlayImage != null)
        {
            var color = _overlayImage.color;
            color.a = 1f;
            _overlayImage.color = color;
        }

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
    }

    private static Task AwaitAsyncOperation(AsyncOperation asyncOperation)
    {
        if (asyncOperation == null || asyncOperation.isDone)
            return Task.CompletedTask;

        var completionSource = new TaskCompletionSource<bool>();
        asyncOperation.completed += _ => completionSource.TrySetResult(true);
        return completionSource.Task;
    }
}
