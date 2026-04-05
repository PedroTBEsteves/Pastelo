using System;
using System.Collections.Generic;
using System.Linq;

public class ClosedPastelDough
{
    private readonly Recipe _recipe;
    private readonly IReadOnlyList<Filling> _fillingSlots;
    private readonly float _timeToIncreaseFriedLevelInSeconds;
    
    private FriedLevel _friedLevel;
    private float _timeFrying;
    
    public ClosedPastelDough(
        Dough dough,
        IReadOnlyDictionary<Filling, int> fillings,
        IReadOnlyList<Filling> fillingSlots,
        float timeToIncreaseFriedLevelInSeconds)
        : this(new Recipe(dough, fillings), fillingSlots, FriedLevel.Raw, timeToIncreaseFriedLevelInSeconds)
    {
    }

    public ClosedPastelDough(
        Recipe recipe,
        IReadOnlyList<Filling> fillingSlots,
        FriedLevel friedLevel,
        float timeToIncreaseFriedLevelInSeconds)
    {
        _recipe = recipe;
        _fillingSlots = fillingSlots?.ToArray() ?? Array.Empty<Filling>();
        _friedLevel = friedLevel;
        _timeToIncreaseFriedLevelInSeconds = timeToIncreaseFriedLevelInSeconds;
    }

    public event Action<FriedLevel> FriedLevelChanged;

    public Dough Dough => _recipe.Dough;
    public Recipe Recipe => _recipe;
    public IReadOnlyList<Filling> FillingSlots => _fillingSlots;
    public FriedLevel FriedLevel => _friedLevel;
    public float FryingProgress => _timeFrying / _timeToIncreaseFriedLevelInSeconds;
    
    public void Fry(float deltaTime)
    {
        _timeFrying += deltaTime;

        if (_timeFrying >= _timeToIncreaseFriedLevelInSeconds && _friedLevel != FriedLevel.Burnt)
        {
            _friedLevel++;
            if (_friedLevel != FriedLevel.Burnt)
                _timeFrying -= _timeToIncreaseFriedLevelInSeconds;
            FriedLevelChanged?.Invoke(_friedLevel);
        }
    }

    public Pastel Finish() => new(_recipe, _fillingSlots, _friedLevel);
}
