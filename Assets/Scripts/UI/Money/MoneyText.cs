using System;
using System.Globalization;
using KBCore.Refs;
using PrimeTween;
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
    private readonly Money _money;

    private void Awake()
    {
        UpdateMoneyText(_money.Amount);
        _money.MoneyChanged += OnMoneyChanged;
    }

    private void OnMoneyChanged(Money.MoneyChangedEvent moneyChanged)
    {
        UpdateMoneyText(moneyChanged.Current);
    }

    private void UpdateMoneyText(float value)
    {
        var text = TextUtils.FormatAsMoney(value);
        _text.SetText(text);
    }
}
