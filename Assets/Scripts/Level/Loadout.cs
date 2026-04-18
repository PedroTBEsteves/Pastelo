using System;
using System.Collections.Generic;

public sealed class Loadout
{
    private readonly HashSet<Dough> _doughs = new();
    private readonly HashSet<Filling> _fillings = new();

    public IReadOnlyCollection<Dough> Doughs => _doughs;
    public IReadOnlyCollection<Filling> Fillings => _fillings;

    public bool AddDough(Dough dough)
    {
        if (dough == null)
            throw new ArgumentNullException(nameof(dough));

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

        return _fillings.Add(filling);
    }

    public bool RemoveFilling(Filling filling)
    {
        if (filling == null)
            throw new ArgumentNullException(nameof(filling));

        return _fillings.Remove(filling);
    }
}
