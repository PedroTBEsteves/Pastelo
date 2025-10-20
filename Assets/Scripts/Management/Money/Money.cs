using System;
using UnityEngine;

public class Money
{
    private float _amount;

    public Money(MoneySettings settings)
    {
        _amount = settings.InitialAmount;
    }

    public float Amount
    {
        get => _amount;
        set
        {
            var previous = _amount;
            _amount = value;
            MoneyChanged(new MoneyChangedEvent(previous, value));
        }
    }

    public event Action<MoneyChangedEvent> MoneyChanged = delegate { }; 
    
    public bool CanSpend(float amount) => Amount >= amount;

    public bool TrySpend(float amount)
    {
        if (!CanSpend(amount))
            return false;

        Amount -= amount;
        return true;
    }
    
    public void Gain(float amount) => Amount += amount;

    public record MoneyChangedEvent(float Previous, float Current);
}
