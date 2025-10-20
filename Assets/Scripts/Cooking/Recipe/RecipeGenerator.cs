using System.Collections.Generic;
using UnityEngine;

public class RecipeGenerator
{
    private readonly int _minFillings;
    private readonly int _maxFillingsInclusive;
    private readonly float _noMainProbability;
    private readonly float _maxMainPercentage;

    private readonly IngredientsStorage _ingredientsStorage;
    
    public RecipeGenerator(RecipeGeneratorSettings settings, IngredientsStorage ingredientsStorage)
    {
        _ingredientsStorage = ingredientsStorage;
        
        _minFillings = settings.MinFillings;
        _maxFillingsInclusive = settings.MaxFillingsInclusive;
        _maxMainPercentage = settings.MaxMainPercentage;
        _noMainProbability = settings.NoMainProbability;
    }

    public Recipe Generate()
    {
        var dough = _ingredientsStorage.Doughs.GetRandomElement();

        var fillings = new Dictionary<Filling, int>();
        
        var fillingsCount = GetFillingsCount();

        if (WillHaveMain() && fillingsCount > 0)
        {
            var main = _ingredientsStorage.Mains.GetRandomElement();
            var mainCount = GetMainsCount(fillingsCount);
            fillingsCount -= mainCount;
            fillings.Add(main, mainCount);
        }
        
        var availableFillings = new List<Filling>(_ingredientsStorage.Sides);
        
        for (var i = 0; i < fillingsCount; i++)
        {
            var side = availableFillings.GetRandomElement();
            availableFillings.Add(side);
            fillings[side] = fillings.GetValueOrDefault(side) + 1;
        }
        
        return new Recipe(dough, fillings);
    }

    private int GetFillingsCount() => Random.Range(_minFillings, _maxFillingsInclusive + 1);
    
    private bool WillHaveMain() => Random.value > _noMainProbability;

    private int GetMainsCount(int fillingsCount) =>
        Mathf.Max(1, Mathf.CeilToInt(Random.Range(0.0f, _maxMainPercentage * fillingsCount)));
}
