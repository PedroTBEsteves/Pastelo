using System;
using System.Collections.Generic;

public sealed class Loadout
{
    private readonly HashSet<Dough> _doughs = new();
    private readonly HashSet<Filling> _fillings = new();

    public Loadout(int maxDoughs, int maxFillings)
    {
        MaxDoughs = Math.Max(0, maxDoughs);
        MaxFillings = Math.Max(0, maxFillings);
    }

    public IReadOnlyCollection<Dough> Doughs => _doughs;
    public IReadOnlyCollection<Filling> Fillings => _fillings;
    public int DoughCount => _doughs.Count;
    public int FillingCount => _fillings.Count;
    public int MaxDoughs { get; }
    public int MaxFillings { get; }

    public bool AddDough(Dough dough)
    {
        if (dough == null)
            throw new ArgumentNullException(nameof(dough));

        if (_doughs.Count >= MaxDoughs)
            return false;

        return _doughs.Add(dough);
    }

    public bool RemoveDough(Dough dough)
    {
        if (dough == null)
            throw new ArgumentNullException(nameof(dough));

        return _doughs.Remove(dough);
    }

    public bool AddFilling(Filling filling)
    {
        if (filling == null)
            throw new ArgumentNullException(nameof(filling));

        if (_fillings.Count >= MaxFillings)
            return false;

        return _fillings.Add(filling);
    }

    public bool RemoveFilling(Filling filling)
    {
        if (filling == null)
            throw new ArgumentNullException(nameof(filling));

        return _fillings.Remove(filling);
    }
}
