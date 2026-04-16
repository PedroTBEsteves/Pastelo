using System.Collections.Generic;
using UnityEngine;

public class RecipeGenerator
{
    private readonly int _minFillings;
    private readonly int _maxFillingsInclusive;

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
    }

    public Recipe Generate()
    {
        if (_tutorialState.IsActive)
            return _tutorialState.TutorialRecipe;
        
        var dough = _ingredientsStorage.Doughs.GetRandomElement();

        var fillings = new Dictionary<Filling, int>();
        var fillingsCount = GetFillingsCount();

        AddFillings(fillings, fillingsCount);
        
        return new Recipe(dough, fillings);
    }
    
    private int GetFillingsCount() => Random.Range(_minFillings, _maxFillingsInclusive + 1);

    private void AddFillings(Dictionary<Filling, int> fillings, int fillingsCount)
    {
        if (fillingsCount <= 0 || _ingredientsStorage.Fillings.Count == 0)
            return;

        var maxSelectableTypes = Mathf.Min(fillingsCount, _ingredientsStorage.Fillings.Count);

        if (maxSelectableTypes <= 0)
            return;

        var selectedTypesCount = Random.Range(1, maxSelectableTypes + 1);
        var selectedFillings = GetRandomUniqueFillings(selectedTypesCount);

        foreach (var filling in selectedFillings)
            fillings[filling] = 1;

        var remainingFillingsCount = fillingsCount - selectedFillings.Count;

        for (var i = 0; i < remainingFillingsCount; i++)
        {
            var filling = selectedFillings.GetRandomElement();
            fillings[filling] = fillings.GetValueOrDefault(filling) + 1;
        }
    }

    private List<Filling> GetRandomUniqueFillings(int count)
    {
        var availableFillings = new List<Filling>(_ingredientsStorage.Fillings);
        var selectedFillings = new List<Filling>(count);

        for (var i = 0; i < count; i++)
        {
            var fillingIndex = Random.Range(0, availableFillings.Count);
            selectedFillings.Add(availableFillings[fillingIndex]);
            availableFillings.RemoveAt(fillingIndex);
        }

        return selectedFillings;
    }
}
