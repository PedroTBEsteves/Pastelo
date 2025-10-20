using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;

public class QuitButton : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private Button _button;

    private void Awake()
    {
        _button.onClick.AddListener(Application.Quit);
    }
}
