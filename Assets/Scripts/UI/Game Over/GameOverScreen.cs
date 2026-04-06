using PrimeTween;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    [Inject]
    private readonly StrikesController _strikesController;

    [Inject]
    private readonly ISceneTransitionService _sceneTransitionService;
    
    [SerializeField]
    private GameObject _gameOverScreen;
    
    [SerializeField]
    private Button _restartButton;

    [SerializeField]
    private TweenSettings<float> _fadeInTweenSettings = new(1f, 0.35f, Ease.OutQuad, useUnscaledTime: true);

    [SerializeField]
    private float _menuLoadDelay = 2f;

    [SerializeField]
    private int _menuSceneIndex;

    private CanvasGroup _gameOverCanvasGroup;
    private Sequence _gameOverSequence;

    private void Awake()
    {
        _strikesController.GameOver += OnGameOver;
        _restartButton.onClick.AddListener(Restart);
        _gameOverCanvasGroup = GetOrCreateCanvasGroup();
        HideGameOverScreen();
    }

    private void OnDestroy()
    {
        _strikesController.GameOver -= OnGameOver;
        _restartButton.onClick.RemoveListener(Restart);
        StopGameOverSequence();
    }

    private void OnGameOver()
    {
        StopGameOverSequence();

        _gameOverScreen.SetActive(true);
        _gameOverCanvasGroup.alpha = 0f;
        _gameOverCanvasGroup.interactable = true;
        _gameOverCanvasGroup.blocksRaycasts = true;

        _gameOverSequence = Sequence.Create(Tween.Alpha(_gameOverCanvasGroup, _fadeInTweenSettings))
            .Chain(Tween.Delay(_menuLoadDelay, LoadMenu, useUnscaledTime: true));
    }

    private void Restart()
    {
        StopGameOverSequence();

        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    private async void LoadMenu()
    {
        await _sceneTransitionService.TryLoadSceneAsync(_menuSceneIndex);
    }

    private CanvasGroup GetOrCreateCanvasGroup()
    {
        return _gameOverScreen.TryGetComponent(out CanvasGroup canvasGroup)
            ? canvasGroup
            : _gameOverScreen.AddComponent<CanvasGroup>();
    }

    private void HideGameOverScreen()
    {
        _gameOverCanvasGroup.alpha = 0f;
        _gameOverCanvasGroup.interactable = false;
        _gameOverCanvasGroup.blocksRaycasts = false;
        _gameOverScreen.SetActive(false);
    }

    private void StopGameOverSequence()
    {
        if (_gameOverSequence.isAlive)
            _gameOverSequence.Stop();
    }
}
