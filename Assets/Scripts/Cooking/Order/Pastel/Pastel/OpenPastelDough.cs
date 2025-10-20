using System.Collections.Generic;

public class OpenPastelDough
{
    private readonly Dough _dough;
    private readonly Dictionary<Filling, int> _fillings = new();
    
    public OpenPastelDough(Dough dough)
    {
        _dough = dough;
    }
    
    public void AddFilling(Filling filling) => _fillings[filling] = _fillings.GetValueOrDefault(filling) + 1;

    public ClosedPastelDough Close(PastelCookingSettings settings)
    {
        return new ClosedPastelDough(_dough, _fillings, settings.TimeToIncreaseFriedLevelInSeconds);
    }
}
