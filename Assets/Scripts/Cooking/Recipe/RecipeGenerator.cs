using System.Collections.Generic;
using UnityEngine;

public class RecipeGenerator
{
    private const int MaxAdditionalTypesWithoutMain = 3;
    private const int MaxAdditionalTypesWithMain = 2;

    private readonly int _minFillings;
    private readonly int _maxFillingsInclusive;
    private readonly float _noMainProbability;
    private readonly float _maxMainPercentage;

    private readonly IngredientsStorage _ingredientsStorage;

    private readonly GameplayTutorialState _tutorialState;
    
    public RecipeGenerator(RecipeGeneratorSettings settings, IngredientsStorage ingredientsStorage, GameplayTutorialState tutorialState)
    {
        _ingredientsStorage = ingredientsStorage;
        _tutorialState = tutorialState;

        var minFillings = Mathf.Max(0, settings.MinFillings);
        var maxFillingsInclusive = Mathf.Max(0, settings.MaxFillingsInclusive);

        _minFillings = Mathf.Min(minFillings, maxFillingsInclusive);
        _maxFillingsInclusive = Mathf.Max(minFillings, maxFillingsInclusive);
        _maxMainPercentage = settings.MaxMainPercentage;
        _noMainProbability = settings.NoMainProbability;
    }

    public Recipe Generate()
    {
        if (_tutorialState.IsActive)
            return _tutorialState.TutorialRecipe;
        
        var dough = _ingredientsStorage.Doughs.GetRandomElement();

        var fillings = new Dictionary<Filling, int>();
        
        var fillingsCount = GetFillingsCount();
        var hasMain = false;

        if (WillHaveMain() && fillingsCount > 0 && _ingredientsStorage.Mains.Count > 0)
        {
            var main = _ingredientsStorage.Mains.GetRandomElement();
            var mainCount = GetMainsCount(fillingsCount);
            fillingsCount -= mainCount;
            fillings.Add(main, mainCount);
            hasMain = true;
        }

        AddSides(fillings, fillingsCount, hasMain);
        
        return new Recipe(dough, fillings);
    }
    
    private int GetFillingsCount() => Random.Range(_minFillings, _maxFillingsInclusive + 1);
    
    private bool WillHaveMain() => Random.value > _noMainProbability;

    private int GetMainsCount(int fillingsCount) =>
        Mathf.Max(1, Mathf.CeilToInt(Random.Range(0.0f, _maxMainPercentage * fillingsCount)));

    private void AddSides(Dictionary<Filling, int> fillings, int sidesCount, bool hasMain)
    {
        if (sidesCount <= 0 || _ingredientsStorage.Sides.Count == 0)
            return;

        var maxAdditionalTypes = hasMain ? MaxAdditionalTypesWithMain : MaxAdditionalTypesWithoutMain;
        var maxSelectableTypes = Mathf.Min(maxAdditionalTypes, sidesCount, _ingredientsStorage.Sides.Count);

        if (maxSelectableTypes <= 0)
            return;

        var selectedTypesCount = Random.Range(1, maxSelectableTypes + 1);
        var selectedSides = GetRandomUniqueSides(selectedTypesCount);

        foreach (var side in selectedSides)
            fillings[side] = 1;

        var remainingSidesCount = sidesCount - selectedSides.Count;

        for (var i = 0; i < remainingSidesCount; i++)
        {
            var side = selectedSides.GetRandomElement();
            fillings[side] = fillings.GetValueOrDefault(side) + 1;
        }
    }

    private List<Side> GetRandomUniqueSides(int count)
    {
        var availableSides = new List<Side>(_ingredientsStorage.Sides);
        var selectedSides = new List<Side>(count);

        for (var i = 0; i < count; i++)
        {
            var sideIndex = Random.Range(0, availableSides.Count);
            selectedSides.Add(availableSides[sideIndex]);
            availableSides.RemoveAt(sideIndex);
        }

        return selectedSides;
    }
}
