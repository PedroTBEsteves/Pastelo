using System;
using System.Collections.Generic;
using System.Linq;

public class Recipe : IEquatable<Recipe>
{
    private readonly Dictionary<Filling, int> _fillings;
    public Recipe(Dough dough, IReadOnlyDictionary<Filling, int> fillings)
    {
        Dough = dough;
        _fillings = new Dictionary<Filling, int>(fillings);
        Value = dough.Value + fillings.Sum(f => f.Key.Value * f.Value);
    }
    
    public Dough Dough { get; }
    public IReadOnlyDictionary<Filling, int> Fillings => _fillings;
    
    public float Value { get; }

    public bool Equals(Recipe other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        
        var equalDoughs = Dough == other.Dough;
        var equalFillings = _fillings.Count ==  other.Fillings.Count && !_fillings.Except(other.Fillings).Any();
        
        return equalDoughs && equalFillings;
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Recipe)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_fillings, Dough);
    }

    public override string ToString()
    {
        return $"{Dough.Name} - ({string.Join(",", _fillings.Select(i => $"({i.Key.Name},{i.Value})"))})";
    }
}
