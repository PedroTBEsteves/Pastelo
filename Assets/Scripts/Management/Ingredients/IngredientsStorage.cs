using System;
using System.Collections.Generic;
using UnityEngine;

public class IngredientsStorage
{
    private readonly List<Dough> _doughs;
    private readonly List<Filling> _fillings;

    private int _currentPriceIndex;
    private readonly IReadOnlyList<float> _prices;

    private readonly MoneyManager _moneyManager;
    
    public IngredientsStorage(IngredientsStorageSettings settings, MoneyManager moneyManager)
    {
        _doughs = new List<Dough>(settings.StartingDoughs);
        _fillings = new List<Filling>(settings.StartingFillings);
        _prices = settings.Prices;
        _moneyManager = moneyManager;
    }

    public event Action<float> PriceChanged = delegate { };
    
    public IReadOnlyList<Dough> Doughs => _doughs;
    public IReadOnlyList<Filling> Fillings => _fillings;

    public float CurrentPrice => _prices[_currentPriceIndex];

    public bool TryBuyIngredient(Ingredient ingredient)
    {
        var spent = _moneyManager.TrySpend(CurrentPrice);
        
        if (spent)
        {
            _currentPriceIndex = Mathf.Min(_currentPriceIndex + 1, _prices.Count - 1);
            PriceChanged(CurrentPrice);
            Unlock(ingredient);
        }
        
        return spent;
    }

    public bool Contains(Ingredient ingredient)
    {
        return ingredient switch
        {
            Filling filling => _fillings.Contains(filling),
            Dough dough => _doughs.Contains(dough),
            _ => throw new InvalidOperationException("Ingredient doesn't exist")
        };
    }

    private void Unlock(Ingredient ingredient)
    {
        switch (ingredient)
        {
            case Filling filling:
                _fillings.Add(filling);
                break;
            case Dough dough:
                _doughs.Add(dough);
                break;
        }
    }
}
