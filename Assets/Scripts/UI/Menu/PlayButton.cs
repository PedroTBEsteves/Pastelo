using KBCore.Refs;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayButton : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private Button _button;
    
    [SerializeField]
    private int _sceneIndex;

    private void Awake()
    {
        _button.onClick.AddListener(() => SceneManager.LoadScene(_sceneIndex));
    }
}
