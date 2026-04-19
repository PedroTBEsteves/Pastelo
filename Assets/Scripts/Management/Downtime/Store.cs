using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public readonly struct StoreFixedIngredientOffer
{
    public StoreFixedIngredientOffer(Ingredient ingredient, int remainingDays, int remainingStock)
    {
        Ingredient = ingredient;
        RemainingDays = Mathf.Max(0, remainingDays);
        RemainingStock = Mathf.Max(0, remainingStock);
    }

    public Ingredient Ingredient { get; }
    public int RemainingDays { get; }
    public int RemainingStock { get; }
}

public sealed class Store
{
    private readonly StoreSettings _settings;
    private readonly DayManager _dayManager;
    private readonly MoneyManager _moneyManager;
    private readonly Inventory _inventory;

    private readonly List<Ingredient> _randomIngredients = new();
    private readonly List<StoreFixedIngredientOffer> _fixedIngredients = new();
    private readonly List<ActiveRandomIngredient> _activeRandomIngredients = new();
    private readonly List<ActiveFixedIngredient> _activeFixedIngredients = new();

    private int _nextFixedSequenceIndex;
    private int _resolvedDay;

    public Store(StoreSettings settings, DayManager dayManager, MoneyManager moneyManager, Inventory inventory)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _dayManager = dayManager ?? throw new ArgumentNullException(nameof(dayManager));
        _moneyManager = moneyManager ?? throw new ArgumentNullException(nameof(moneyManager));
        _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
    }

    public IReadOnlyList<Ingredient> RandomIngredients
    {
        get
        {
            EnsureOffersResolved();
            return _randomIngredients;
        }
    }

    public IReadOnlyList<StoreFixedIngredientOffer> FixedIngredients
    {
        get
        {
            EnsureOffersResolved();
            return _fixedIngredients;
        }
    }

    public bool TryBuyIngredient(Ingredient ingredient)
    {
        if (ingredient == null)
            throw new ArgumentNullException(nameof(ingredient));

        EnsureOffersResolved();

        if (!HasStock(ingredient) || !_moneyManager.TrySpend(ingredient.BuyPrice))
            return false;

        ConsumeStock(ingredient);
        _inventory.Add(ingredient);
        return true;
    }

    public bool HasStock(Ingredient ingredient)
    {
        if (ingredient == null)
            return false;

        EnsureOffersResolved();
        return TryGetRemainingStock(ingredient, out var remainingStock) && remainingStock > 0;
    }

    public int GetRemainingStock(Ingredient ingredient)
    {
        if (ingredient == null)
            return 0;

        EnsureOffersResolved();
        return TryGetRemainingStock(ingredient, out var remainingStock) ? remainingStock : 0;
    }

    private void EnsureOffersResolved()
    {
        var currentDay = Mathf.Max(1, _dayManager.CurrentDay);

        if (_resolvedDay == 0)
        {
            ResolveDay(1);
            _resolvedDay = 1;
        }

        while (_resolvedDay < currentDay)
        {
            ResolveDay(_resolvedDay + 1);
            _resolvedDay++;
        }
    }

    private void ResolveDay(int day)
    {
        if (day <= 1)
            _activeFixedIngredients.Clear();
        else
            RemoveInactiveFixedIngredients(day);

        AddFixedIngredientsForDay(day);
        ResolveRandomIngredients();
        RebuildPublicOffers(day);
    }

    private void RemoveInactiveFixedIngredients(int currentDay)
    {
        for (var i = _activeFixedIngredients.Count - 1; i >= 0; i--)
        {
            var entry = _activeFixedIngredients[i];
            if (entry.ExpireOnDay <= currentDay || entry.RemainingStock <= 0)
                _activeFixedIngredients.RemoveAt(i);
        }
    }

    private void AddFixedIngredientsForDay(int currentDay)
    {
        var fixedSequence = _settings.FixedSequence;
        var fixedIngredientsCount = Mathf.Max(0, _settings.FixedIngredientsCount);

        if (fixedSequence == null || fixedSequence.Count == 0 || fixedIngredientsCount == 0)
            return;

        var hasAnyIngredient = false;
        for (var i = 0; i < fixedSequence.Count; i++)
        {
            if (fixedSequence[i].Ingredient == null)
                continue;

            hasAnyIngredient = true;
            break;
        }

        if (!hasAnyIngredient)
            return;

        while (_activeFixedIngredients.Count < fixedIngredientsCount)
        {
            var sequenceEntry = fixedSequence[_nextFixedSequenceIndex];
            _nextFixedSequenceIndex = (_nextFixedSequenceIndex + 1) % fixedSequence.Count;

            if (sequenceEntry.Ingredient == null)
                continue;

            _activeFixedIngredients.Add(new ActiveFixedIngredient(
                sequenceEntry.Ingredient,
                currentDay + sequenceEntry.DurationInDays,
                sequenceEntry.DurationInDays));
        }
    }

    private void ResolveRandomIngredients()
    {
        _activeRandomIngredients.Clear();

        var randomPool = _settings.RandomPool;
        var randomIngredientsCount = Mathf.Max(0, _settings.RandomIngredientsCount);

        if (randomPool == null || randomPool.Count == 0 || randomIngredientsCount == 0)
            return;

        var candidates = new List<Ingredient>(randomPool.Count);
        var selectedLookup = new HashSet<Ingredient>();

        for (var i = 0; i < _activeFixedIngredients.Count; i++)
            selectedLookup.Add(_activeFixedIngredients[i].Ingredient);

        for (var i = 0; i < randomPool.Count; i++)
        {
            var ingredient = randomPool[i];

            if (ingredient == null || !selectedLookup.Add(ingredient))
                continue;

            candidates.Add(ingredient);
        }

        for (var i = candidates.Count - 1; i > 0; i--)
        {
            var swapIndex = Random.Range(0, i + 1);
            (candidates[i], candidates[swapIndex]) = (candidates[swapIndex], candidates[i]);
        }

        var selectedCount = Mathf.Min(randomIngredientsCount, candidates.Count);
        var minStock = _settings.GetNormalizedRandomItemMinStock();
        var maxStock = _settings.GetNormalizedRandomItemMaxStock();

        for (var i = 0; i < selectedCount; i++)
        {
            _activeRandomIngredients.Add(new ActiveRandomIngredient(
                candidates[i],
                Random.Range(minStock, maxStock + 1)));
        }
    }

    private void RebuildPublicOffers(int day)
    {
        _randomIngredients.Clear();
        _fixedIngredients.Clear();

        for (var i = 0; i < _activeRandomIngredients.Count; i++)
            _randomIngredients.Add(_activeRandomIngredients[i].Ingredient);

        for (var i = 0; i < _activeFixedIngredients.Count; i++)
        {
            var entry = _activeFixedIngredients[i];
            _fixedIngredients.Add(new StoreFixedIngredientOffer(
                entry.Ingredient,
                entry.ExpireOnDay - day,
                entry.RemainingStock));
        }
    }

    private bool TryGetRemainingStock(Ingredient ingredient, out int remainingStock)
    {
        for (var i = 0; i < _activeRandomIngredients.Count; i++)
        {
            if (_activeRandomIngredients[i].Ingredient != ingredient)
                continue;

            remainingStock = _activeRandomIngredients[i].RemainingStock;
            return true;
        }

        for (var i = 0; i < _activeFixedIngredients.Count; i++)
        {
            if (_activeFixedIngredients[i].Ingredient != ingredient)
                continue;

            remainingStock = _activeFixedIngredients[i].RemainingStock;
            return true;
        }

        remainingStock = 0;
        return false;
    }

    private void ConsumeStock(Ingredient ingredient)
    {
        for (var i = 0; i < _activeRandomIngredients.Count; i++)
        {
            if (_activeRandomIngredients[i].Ingredient != ingredient)
                continue;

            var entry = _activeRandomIngredients[i];
            _activeRandomIngredients[i] = entry.WithRemainingStock(entry.RemainingStock - 1);
            RebuildPublicOffers(Mathf.Max(1, _dayManager.CurrentDay));
            return;
        }

        for (var i = 0; i < _activeFixedIngredients.Count; i++)
        {
            if (_activeFixedIngredients[i].Ingredient != ingredient)
                continue;

            var entry = _activeFixedIngredients[i];
            _activeFixedIngredients[i] = entry.WithRemainingStock(entry.RemainingStock - 1);
            RebuildPublicOffers(Mathf.Max(1, _dayManager.CurrentDay));
            return;
        }
    }

    private readonly struct ActiveRandomIngredient
    {
        public ActiveRandomIngredient(Ingredient ingredient, int remainingStock)
        {
            Ingredient = ingredient;
            RemainingStock = Mathf.Max(0, remainingStock);
        }

        public Ingredient Ingredient { get; }
        public int RemainingStock { get; }

        public ActiveRandomIngredient WithRemainingStock(int remainingStock)
        {
            return new ActiveRandomIngredient(Ingredient, remainingStock);
        }
    }

    private readonly struct ActiveFixedIngredient
    {
        public ActiveFixedIngredient(Ingredient ingredient, int expireOnDay, int remainingStock)
        {
            Ingredient = ingredient;
            ExpireOnDay = expireOnDay;
            RemainingStock = Mathf.Max(0, remainingStock);
        }

        public Ingredient Ingredient { get; }
        public int ExpireOnDay { get; }
        public int RemainingStock { get; }

        public ActiveFixedIngredient WithRemainingStock(int remainingStock)
        {
            return new ActiveFixedIngredient(Ingredient, ExpireOnDay, remainingStock);
        }
    }
}
