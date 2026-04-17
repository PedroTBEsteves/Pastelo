using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class Store
{
    private readonly StoreSettings _settings;
    private readonly DayManager _dayManager;
    private readonly MoneyManager _moneyManager;
    private readonly Inventory _inventory;

    private readonly List<Ingredient> _randomIngredients = new();
    private readonly List<Ingredient> _fixedIngredients = new();

    private int _resolvedDay = -1;

    public Store(StoreSettings settings, DayManager dayManager, MoneyManager moneyManager, Inventory inventory)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _dayManager = dayManager ?? throw new ArgumentNullException(nameof(dayManager));
        _moneyManager = moneyManager ?? throw new ArgumentNullException(nameof(moneyManager));
        _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));

        var a = RandomIngredients;
        var b = FixedIngredients;
    }

    public IReadOnlyList<Ingredient> RandomIngredients
    {
        get
        {
            EnsureOffersResolved();
            return _randomIngredients;
        }
    }

    public IReadOnlyList<Ingredient> FixedIngredients
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

        if (!IsIngredientAvailable(ingredient) || !_moneyManager.TrySpend(ingredient.BuyPrice))
            return false;

        _inventory.Add(ingredient);
        return true;
    }

    private void EnsureOffersResolved()
    {
        var currentDay = Mathf.Max(1, _dayManager.CurrentDay);

        if (_resolvedDay == currentDay)
            return;

        ResolveFixedIngredients(currentDay);
        ResolveRandomIngredients(currentDay);
        _resolvedDay = currentDay;
    }

    private void ResolveFixedIngredients(int day)
    {
        _fixedIngredients.Clear();

        var fixedSequence = _settings.FixedSequence;
        var fixedIngredientsCount = Mathf.Max(0, _settings.FixedIngredientsCount);

        if (fixedSequence == null || fixedSequence.Count == 0 || fixedIngredientsCount == 0)
            return;

        var activeEntries = new List<ActiveFixedIngredient>(fixedIngredientsCount);
        var nextSequenceIndex = 0;

        AddFixedIngredientsForDay(activeEntries, ref nextSequenceIndex, fixedIngredientsCount, 1);

        for (var currentDay = 2; currentDay <= day; currentDay++)
        {
            RemoveExpiredFixedIngredients(activeEntries, currentDay);
            AddFixedIngredientsForDay(activeEntries, ref nextSequenceIndex, fixedIngredientsCount, currentDay);
        }

        for (var i = 0; i < activeEntries.Count; i++)
            _fixedIngredients.Add(activeEntries[i].Ingredient);
    }

    private void ResolveRandomIngredients(int day)
    {
        _randomIngredients.Clear();

        var randomPool = _settings.RandomPool;
        var randomIngredientsCount = Mathf.Max(0, _settings.RandomIngredientsCount);

        if (randomPool == null || randomPool.Count == 0 || randomIngredientsCount == 0)
            return;

        var candidates = new List<Ingredient>(randomPool.Count);
        var selectedLookup = new HashSet<Ingredient>(_fixedIngredients);

        for (var i = 0; i < randomPool.Count; i++)
        {
            var ingredient = randomPool[i];

            if (ingredient == null || !selectedLookup.Add(ingredient))
                continue;

            candidates.Add(ingredient);
        }

        var random = new System.Random(unchecked(day * 486187739 + 1203941));

        for (var i = candidates.Count - 1; i > 0; i--)
        {
            var swapIndex = random.Next(i + 1);
            (candidates[i], candidates[swapIndex]) = (candidates[swapIndex], candidates[i]);
        }

        var selectedCount = Mathf.Min(randomIngredientsCount, candidates.Count);

        for (var i = 0; i < selectedCount; i++)
            _randomIngredients.Add(candidates[i]);
    }

    private bool IsIngredientAvailable(Ingredient ingredient)
    {
        return _fixedIngredients.Contains(ingredient) || _randomIngredients.Contains(ingredient);
    }

    private void RemoveExpiredFixedIngredients(List<ActiveFixedIngredient> activeEntries, int currentDay)
    {
        for (var i = activeEntries.Count - 1; i >= 0; i--)
        {
            if (activeEntries[i].ExpireOnDay <= currentDay)
                activeEntries.RemoveAt(i);
        }
    }

    private void AddFixedIngredientsForDay(
        List<ActiveFixedIngredient> activeEntries,
        ref int nextSequenceIndex,
        int targetCount,
        int currentDay)
    {
        var fixedSequence = _settings.FixedSequence;

        while (activeEntries.Count < targetCount)
        {
            var sequenceEntry = fixedSequence[nextSequenceIndex];
            nextSequenceIndex = (nextSequenceIndex + 1) % fixedSequence.Count;

            if (sequenceEntry.Ingredient == null)
                continue;

            activeEntries.Add(new ActiveFixedIngredient(
                sequenceEntry.Ingredient,
                currentDay + sequenceEntry.DurationInDays));
        }
    }

    private readonly struct ActiveFixedIngredient
    {
        public ActiveFixedIngredient(Ingredient ingredient, int expireOnDay)
        {
            Ingredient = ingredient;
            ExpireOnDay = expireOnDay;
        }

        public Ingredient Ingredient { get; }
        public int ExpireOnDay { get; }
    }
}
