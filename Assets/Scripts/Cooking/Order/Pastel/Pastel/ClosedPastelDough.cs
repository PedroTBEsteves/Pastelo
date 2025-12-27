using System;
using System.Collections.Generic;

public class ClosedPastelDough
{
    private readonly Recipe _recipe;
    private readonly float _timeToIncreaseFriedLevelInSeconds;
    
    private FriedLevel _friedLevel;
    private float _timeFrying;
    
    public ClosedPastelDough(Dough dough, IReadOnlyDictionary<Filling, int> fillings, float timeToIncreaseFriedLevelInSeconds)
    {
        _recipe = new Recipe(dough, fillings);
        _timeToIncreaseFriedLevelInSeconds = timeToIncreaseFriedLevelInSeconds;
    }

    public event Action<FriedLevel> FriedLevelChanged;

    public Dough Dough => _recipe.Dough;
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

    public Pastel Finish() => new(_recipe, _friedLevel);
}
