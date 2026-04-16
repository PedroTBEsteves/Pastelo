using Cysharp.Threading.Tasks;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionButton : ValidatedMonoBehaviour
{
    [Inject]
    private readonly LevelSelector _levelSelector;

    [SerializeField, Self]
    private Button _button;

    [SerializeField]
    private Level _level;

    private void Awake()
    {
        _button.onClick.AddListener(OnButtonClicked);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        _levelSelector.PlayLevel(_level).Forget();
    }
}
