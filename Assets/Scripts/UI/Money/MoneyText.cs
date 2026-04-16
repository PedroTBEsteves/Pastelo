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

    [SerializeField]
    private AudioSource _moneyGainedAudio;
    
    [SerializeField]
    private AudioSource _moneyLostAudio;
    
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
        
        if (moneyChanged.Current > moneyChanged.Previous)
            _moneyGainedAudio.Play();
    }

    private void UpdateMoneyText(float value)
    {
        var text = ((int)value).ToString();
        _text.SetText(text);
    }
}
