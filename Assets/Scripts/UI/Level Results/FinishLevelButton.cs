using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

public sealed class FinishLevelButton : ValidatedMonoBehaviour
{
    [Inject]
    private readonly DayManager _dayManager;

    [SerializeField, Self]
    private Button _button;

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
        _dayManager.FinishDay();
    }
}
