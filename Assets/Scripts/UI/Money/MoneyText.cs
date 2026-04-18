using KBCore.Refs;
using Reflex.Attributes;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class MoneyText : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private TextMeshProUGUI _text;

    [SerializeField]
    private Color _loseMoneyColor;
    
    [SerializeField]
    private Color _gainedMoneyColor;
    
    [Inject]
    private readonly MoneyManager _moneyManager;

    private void Awake()
    {
        UpdateMoneyText(_moneyManager.Amount);
        _moneyManager.MoneyChanged += OnMoneyChanged;
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
