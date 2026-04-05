using KBCore.Refs;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeSceneButton : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private Button _button;
    
    [SerializeField]
    private int _sceneIndex;

    private void Awake()
    {
        _button.onClick.AddListener(() =>
        {
            if (_sceneIndex == 1)
                GameplayTutorialOptions.SetShouldRunTutorial(true);
            
            SceneManager.LoadScene(_sceneIndex);
        });
    }
}
