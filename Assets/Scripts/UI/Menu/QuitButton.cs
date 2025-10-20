using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;

public class QuitButton : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private Button _button;

    private void Awake()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            gameObject.SetActive(false);
        }
        
        _button.onClick.AddListener(Application.Quit);
    }
}
