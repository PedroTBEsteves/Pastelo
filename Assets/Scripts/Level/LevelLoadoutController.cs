using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class LevelLoadoutController
{
    private const string LevelsResourcePath = "Levels";

    private readonly Dictionary<Level, Loadout> _loadoutsByLevel = new();
    
    private readonly Inventory _inventory;
    private readonly LoadoutSettings _settings;
    private readonly MoneyManager _moneyManager;

    private int _doughUpgradeLevelIndex;
    private int _fillingUpgradeLevelIndex;

    public event Action<Level> LoadoutChanged = delegate { };

    public GameObject CurrentDoughsAreaPrefab => _settings.DoughLevels[_doughUpgradeLevelIndex].AreaPrefab;
    public GameObject CurrentFillingsAreaPrefab => _settings.FillingLevels[_fillingUpgradeLevelIndex].AreaPrefab;
    public GameObject MaxDoughsAreaPrefab => _settings.DoughLevels[^1].AreaPrefab;
    public GameObject MaxFillingsAreaPrefab => _settings.FillingLevels[^1].AreaPrefab;
    public int CurrentDoughUpgradeSize => GetCurrentMaxDoughs();
    public int CurrentFillingUpgradeSize => GetCurrentMaxFillings();
    public int MaxDoughUpgradeSize => _settings.DoughLevels[^1].MaxAmount;
    public int MaxFillingUpgradeSize => _settings.FillingLevels[^1].MaxAmount;

    public LevelLoadoutController(Inventory inventory, LoadoutSettings settings, MoneyManager moneyManager)
    {
        _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _moneyManager = moneyManager ?? throw new ArgumentNullException(nameof(moneyManager));

        ValidateSettings();

        var levels = Resources.LoadAll<Level>(LevelsResourcePath);

        foreach (var level in levels)
        {
            if (level == null || _loadoutsByLevel.ContainsKey(level))
                continue;

            _loadoutsByLevel.Add(level, CreateLoadout());
        }
    }

    public bool CanPurchaseDoughUpgrade()
    {
        return HasNextDoughUpgrade() && _moneyManager.CanSpend(GetDoughUpgradePrice());
    }

    public bool HasNextDoughUpgrade()
    {
        return _doughUpgradeLevelIndex + 1 < _settings.DoughLevels.Count;
    }

    public bool TryGetNextDoughUpgradeSize(out int size)
    {
        if (!HasNextDoughUpgrade())
        {
            size = 0;
            return false;
        }

        size = _settings.DoughLevels[_doughUpgradeLevelIndex + 1].MaxAmount;
        return true;
    }

    public bool TryPurchaseDoughUpgrade()
    {
        if (!HasNextDoughUpgrade())
            return false;

        if (!_moneyManager.TrySpend(GetDoughUpgradePrice()))
            return false;

        _doughUpgradeLevelIndex++;
        ApplyCurrentLimitsToLoadouts();
        NotifyAllLoadoutsChanged();
        return true;
    }

    public float GetDoughUpgradePrice()
    {
        return HasNextDoughUpgrade() ? _settings.DoughLevels[_doughUpgradeLevelIndex + 1].PurchasePrice : 0f;
    }

    public bool CanPurchaseFillingUpgrade()
    {
        return HasNextFillingUpgrade() && _moneyManager.CanSpend(GetFillingUpgradePrice());
    }

    public bool HasNextFillingUpgrade()
    {
        return _fillingUpgradeLevelIndex + 1 < _settings.FillingLevels.Count;
    }

    public bool TryGetNextFillingUpgradeSize(out int size)
    {
        if (!HasNextFillingUpgrade())
        {
            size = 0;
            return false;
        }

        size = _settings.FillingLevels[_fillingUpgradeLevelIndex + 1].MaxAmount;
        return true;
    }

    public bool TryPurchaseFillingUpgrade()
    {
        if (!HasNextFillingUpgrade())
            return false;

        if (!_moneyManager.TrySpend(GetFillingUpgradePrice()))
            return false;

        _fillingUpgradeLevelIndex++;
        ApplyCurrentLimitsToLoadouts();
        NotifyAllLoadoutsChanged();
        return true;
    }

    public float GetFillingUpgradePrice()
    {
        return HasNextFillingUpgrade() ? _settings.FillingLevels[_fillingUpgradeLevelIndex + 1].PurchasePrice : 0f;
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

    public void ReplaceLoadout(Level level, IEnumerable<Dough> doughs, IEnumerable<Filling> fillings)
    {
        if (level == null)
            throw new ArgumentNullException(nameof(level));

        if (doughs == null)
            throw new ArgumentNullException(nameof(doughs));

        if (fillings == null)
            throw new ArgumentNullException(nameof(fillings));

        var normalizedDoughs = doughs.Where(dough => dough != null).Distinct().ToArray();
        var normalizedFillings = fillings.Where(filling => filling != null).Distinct().ToArray();
        var loadout = GetLoadout(level);

        if (normalizedDoughs.Length > loadout.MaxDoughs)
            throw new InvalidOperationException(
                $"Configured dough count '{normalizedDoughs.Length}' exceeds max loadout size '{loadout.MaxDoughs}' for level '{level.name}'.");

        if (normalizedFillings.Length > loadout.MaxFillings)
            throw new InvalidOperationException(
                $"Configured filling count '{normalizedFillings.Length}' exceeds max loadout size '{loadout.MaxFillings}' for level '{level.name}'.");

        loadout.Clear();

        foreach (var dough in normalizedDoughs)
        {
            if (!loadout.AddDough(dough))
                throw new InvalidOperationException($"Failed to add dough '{dough.name}' to loadout for level '{level.name}'.");
        }

        foreach (var filling in normalizedFillings)
        {
            if (!loadout.AddFilling(filling))
                throw new InvalidOperationException($"Failed to add filling '{filling.name}' to loadout for level '{level.name}'.");
        }

        LoadoutChanged(level);
    }

    private Loadout CreateLoadout()
    {
        return new Loadout(GetCurrentMaxDoughs(), GetCurrentMaxFillings());
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

    private int GetCurrentMaxDoughs()
    {
        return _settings.DoughLevels[_doughUpgradeLevelIndex].MaxAmount;
    }

    private int GetCurrentMaxFillings()
    {
        return _settings.FillingLevels[_fillingUpgradeLevelIndex].MaxAmount;
    }

    private void ApplyCurrentLimitsToLoadouts()
    {
        var maxDoughs = GetCurrentMaxDoughs();
        var maxFillings = GetCurrentMaxFillings();

        foreach (var loadout in _loadoutsByLevel.Values)
            loadout.SetLimits(maxDoughs, maxFillings);
    }

    private void NotifyAllLoadoutsChanged()
    {
        foreach (var level in _loadoutsByLevel.Keys)
            LoadoutChanged(level);
    }

    private void ValidateSettings()
    {
        if (_settings.DoughLevels == null || _settings.DoughLevels.Count == 0)
            throw new InvalidOperationException($"{nameof(LoadoutSettings)} must have at least one dough level.");

        if (_settings.FillingLevels == null || _settings.FillingLevels.Count == 0)
            throw new InvalidOperationException($"{nameof(LoadoutSettings)} must have at least one filling level.");
    }
}
