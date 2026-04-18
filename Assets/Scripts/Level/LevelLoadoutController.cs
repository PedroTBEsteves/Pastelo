using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class LevelLoadoutController
{
    private const string LevelsResourcePath = "Levels";

    private readonly Dictionary<Level, Loadout> _loadoutsByLevel = new();
    
    private readonly Inventory _inventory;

    public LevelLoadoutController(Inventory inventory)
    {
        _inventory = inventory;
        var levels = Resources.LoadAll<Level>(LevelsResourcePath);

        foreach (var level in levels)
        {
            if (level == null || _loadoutsByLevel.ContainsKey(level))
                continue;

            _loadoutsByLevel.Add(level, new Loadout());
        }
    }
    
    public Loadout GetLoadout(Level level)
    {
        if (level == null)
            throw new ArgumentNullException(nameof(level));

        return _loadoutsByLevel.TryGetValue(level, out var loadout)
            ? loadout
            : new Loadout();
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
}
