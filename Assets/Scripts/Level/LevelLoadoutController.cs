using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class LevelLoadoutController
{
    private const string LevelsResourcePath = "Levels";

    private readonly Dictionary<Level, Loadout> _loadoutsByLevel = new();
    
    private readonly Inventory _inventory;
    
    private int _maxDoughsPerLoadout = 3;
    private int _maxFillingsPerLoadout = 6;

    public event Action<Level> LoadoutChanged = delegate { };

    public LevelLoadoutController(Inventory inventory)
    {
        _inventory = inventory;
        var levels = Resources.LoadAll<Level>(LevelsResourcePath);

        foreach (var level in levels)
        {
            if (level == null || _loadoutsByLevel.ContainsKey(level))
                continue;

            _loadoutsByLevel.Add(level, CreateLoadout());
        }
    }
    
    public Loadout GetLoadout(Level level)
    {
        if (level == null)
            throw new ArgumentNullException(nameof(level));

        if (_loadoutsByLevel.TryGetValue(level, out var loadout))
            return loadout;

        loadout = CreateLoadout();
        _loadoutsByLevel[level] = loadout;
        return loadout;
    }

    public bool CanConsumeLoadout(Level level)
    {
        var loadout = GetLoadout(level);

        return loadout.Doughs.All(dough => _inventory.Contains(dough)) && loadout.Fillings.All(filling => _inventory.Contains(filling));
    }

    public void ConsumeLoadout(Level level)
    {
        var loadout = GetLoadout(level);

        foreach (var dough in loadout.Doughs)
            _inventory.Remove(dough);

        foreach (var filling in loadout.Fillings)
            _inventory.Remove(filling);
    }

    public bool TryAddDough(Level level, Dough dough)
    {
        if (dough == null)
            throw new ArgumentNullException(nameof(dough));

        return ChangeLoadout(level, loadout => loadout.AddDough(dough));
    }

    public bool TryRemoveDough(Level level, Dough dough)
    {
        if (dough == null)
            throw new ArgumentNullException(nameof(dough));

        return ChangeLoadout(level, loadout => loadout.RemoveDough(dough));
    }

    public bool TryAddFilling(Level level, Filling filling)
    {
        if (filling == null)
            throw new ArgumentNullException(nameof(filling));

        return ChangeLoadout(level, loadout => loadout.AddFilling(filling));
    }

    public bool TryRemoveFilling(Level level, Filling filling)
    {
        if (filling == null)
            throw new ArgumentNullException(nameof(filling));

        return ChangeLoadout(level, loadout => loadout.RemoveFilling(filling));
    }

    public IReadOnlyList<LoadoutInventoryProjectionEntry> GetProjectedInventory(Level level)
    {
        var loadout = GetLoadout(level);
        var availableByIngredient = new Dictionary<Ingredient, int>();

        foreach (var slot in _inventory.Slots)
        {
            if (slot?.Item is not Ingredient ingredient)
                continue;

            availableByIngredient[ingredient] = availableByIngredient.GetValueOrDefault(ingredient) + slot.Quantity;
        }

        var ingredients = new HashSet<Ingredient>(availableByIngredient.Keys);
        ingredients.UnionWith(loadout.Doughs);
        ingredients.UnionWith(loadout.Fillings);

        return ingredients
            .OrderBy(GetIngredientSortOrder)
            .ThenBy(ingredient => ingredient.GetDisplayName())
            .Select(ingredient =>
            {
                var quantity = availableByIngredient.GetValueOrDefault(ingredient);
                var isInLoadout = IsInLoadout(loadout, ingredient);
                var availableQuantity = isInLoadout ? Mathf.Max(0, quantity - 1) : quantity;
                return new LoadoutInventoryProjectionEntry(ingredient, availableQuantity);
            })
            .ToArray();
    }

    public IReadOnlyList<LoadoutMissingIngredientEntry> GetMissingIngredients(Level level)
    {
        var loadout = GetLoadout(level);
        var missingEntries = new List<LoadoutMissingIngredientEntry>(loadout.DoughCount + loadout.FillingCount);

        foreach (var dough in loadout.Doughs)
            missingEntries.Add(new LoadoutMissingIngredientEntry(dough, !_inventory.Contains(dough)));

        foreach (var filling in loadout.Fillings)
            missingEntries.Add(new LoadoutMissingIngredientEntry(filling, !_inventory.Contains(filling)));

        return missingEntries;
    }

    private Loadout CreateLoadout()
    {
        return new Loadout(_maxDoughsPerLoadout, _maxFillingsPerLoadout);
    }

    private bool ChangeLoadout(Level level, Func<Loadout, bool> mutator)
    {
        if (level == null)
            throw new ArgumentNullException(nameof(level));

        var loadout = GetLoadout(level);
        var changed = mutator(loadout);

        if (changed)
            LoadoutChanged(level);

        return changed;
    }

    private static bool IsInLoadout(Loadout loadout, Ingredient ingredient)
    {
        return ingredient switch
        {
            Dough dough => loadout.Doughs.Contains(dough),
            Filling filling => loadout.Fillings.Contains(filling),
            _ => false
        };
    }

    private static int GetIngredientSortOrder(Ingredient ingredient)
    {
        return ingredient switch
        {
            Dough => 0,
            Filling => 1,
            _ => 2
        };
    }
}
