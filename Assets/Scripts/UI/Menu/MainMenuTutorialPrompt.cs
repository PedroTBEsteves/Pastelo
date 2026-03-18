using KBCore.Refs;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuTutorialPrompt : ValidatedMonoBehaviour
{
    [SerializeField]
    private Button _playButton;

    [SerializeField]
    private GameObject _tutorialChoicePanel;

    [SerializeField]
    private Button _withTutorialButton;

    [SerializeField]
    private Button _withoutTutorialButton;

    [SerializeField]
    private int _sceneIndex = 1;

    private void Awake()
    {
        _tutorialChoicePanel.SetActive(false);
        _playButton.onClick.AddListener(OpenPrompt);
        _withTutorialButton.onClick.AddListener(PlayWithTutorial);
        _withoutTutorialButton.onClick.AddListener(PlayWithoutTutorial);
    }

    private void OpenPrompt()
    {
        _tutorialChoicePanel.SetActive(true);
    }

    private void PlayWithTutorial()
    {
        GameplayTutorialOptions.SetShouldRunTutorial(true);
        SceneManager.LoadScene(_sceneIndex);
    }

    private void PlayWithoutTutorial()
    {
        GameplayTutorialOptions.SetShouldRunTutorial(false);
        SceneManager.LoadScene(_sceneIndex);
    }
}
