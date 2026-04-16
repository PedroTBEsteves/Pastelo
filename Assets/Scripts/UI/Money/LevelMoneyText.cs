using KBCore.Refs;
using Reflex.Attributes;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LevelMoneyText : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private TextMeshProUGUI _text;

    [Inject]
    private readonly LevelMoneyManager _levelMoneyManager;

    private void Awake()
    {
        UpdateMoneyText(_levelMoneyManager.Amount);
        _levelMoneyManager.MoneyChanged += OnMoneyChanged;
    }

    private void OnDestroy()
    {
        _levelMoneyManager.MoneyChanged -= OnMoneyChanged;
    }

    private void OnMoneyChanged(MoneyChangedEvent moneyChanged)
    {
        UpdateMoneyText(moneyChanged.Current);
    }

    private void UpdateMoneyText(float value)
    {
        var text = TextUtils.FormatAsMoney(value);
        _text.SetText(text);
    }
}
