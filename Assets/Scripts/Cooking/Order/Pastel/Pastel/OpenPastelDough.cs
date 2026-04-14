using System.Collections.Generic;

public class OpenPastelDough
{
    private readonly Dough _dough;
    private readonly int _maxFillingsInclusive;
    private readonly Dictionary<Filling, int> _fillings = new();
    private int _fillingsCount;
    
    public OpenPastelDough(Dough dough, int maxFillingsInclusive)
    {
        _dough = dough;
        _maxFillingsInclusive = maxFillingsInclusive < 0 ? 0 : maxFillingsInclusive;
    }
    
    public bool TryAddFilling(Filling filling)
    {
        if (_fillingsCount >= _maxFillingsInclusive)
            return false;

        _fillings[filling] = _fillings.GetValueOrDefault(filling) + 1;
        _fillingsCount++;
        return true;
    }

    public bool TryRemoveFilling(Filling filling)
    {
        if (!_fillings.TryGetValue(filling, out var fillingsCount) || fillingsCount <= 0)
            return false;

        if (fillingsCount == 1)
            _fillings.Remove(filling);
        else
            _fillings[filling] = fillingsCount - 1;

        _fillingsCount--;
        return true;
    }

    public ClosedPastelDough Close(PastelCookingSettings settings, IReadOnlyList<Filling> fillingSlots)
    {
        return new ClosedPastelDough(_dough, _fillings, fillingSlots, settings.TimeToIncreaseFriedLevelInSeconds);
    }
}
