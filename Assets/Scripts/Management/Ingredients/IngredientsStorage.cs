using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IngredientsStorage
{
    private readonly List<Dough> _doughs;
    private readonly List<Main> _mains;
    private readonly List<Side> _sides;

    private int _currentPriceIndex;
    private readonly IReadOnlyList<float> _prices;

    private readonly Money _money;
    
    public IngredientsStorage(IngredientsStorageSettings settings, Money money)
    {
        _doughs = new List<Dough>(settings.StartingDoughs);
        _mains = new List<Main>(settings.StartingFillings.OfType<Main>());
        _sides = new List<Side>(settings.StartingFillings.OfType<Side>());
        _prices = settings.Prices;
        _money = money;
    }

    public event Action<float> PriceChanged = delegate { };
    
    public IReadOnlyList<Dough> Doughs => _doughs;
    public IReadOnlyList<Main> Mains => _mains;
    public IReadOnlyList<Side> Sides => _sides;

    public float CurrentPrice => _prices[_currentPriceIndex];

    public bool TryBuyIngredient(Ingredient ingredient)
    {
        var spent = _money.TrySpend(CurrentPrice);
        
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
            Main main => _mains.Contains(main),
            Side side => _sides.Contains(side),
            Dough dough => _doughs.Contains(dough),
            _ => throw new InvalidOperationException("Ingredient doesn't exist")
        };
    }

    private void Unlock(Ingredient ingredient)
    {
        switch (ingredient)
        {
            case Main main:
                _mains.Add(main);
                break;
            case Side side:
                _sides.Add(side);
                break;
            case Dough dough:
                _doughs.Add(dough);
                break;
        }
    }
}
