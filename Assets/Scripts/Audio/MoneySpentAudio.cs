using System;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MoneySpentAudio : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private AudioSource _audioSource;
    
    [Inject]
    private MoneyManager _moneyManager;

    private void Awake()
    {
        _moneyManager.MoneyChanged += OnMoneyChanged;
    }

    private void OnDestroy()
    {
        _moneyManager.MoneyChanged -= OnMoneyChanged;
    }

    private void OnMoneyChanged(MoneyChangedEvent moneyChangedEvent)
    {
        if (moneyChangedEvent.Current < moneyChangedEvent.Previous)
            _audioSource.Play();
    }
}
