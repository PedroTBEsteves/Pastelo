using Reflex.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    [Inject]
    private readonly StrikesController _strikesController;
    
    [SerializeField]
    private Button _restartButton;

    private void Awake()
    {
        _strikesController.GameOver += OnGameOver;
        _restartButton.onClick.AddListener(Restart);
        gameObject.SetActive(false);
    }

    private void OnGameOver()
    {
        gameObject.SetActive(true);
    }

    private static void Restart()
    {
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }
}
