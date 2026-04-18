using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class RecipeGenerator
{
    private readonly int _minFillings;
    private readonly int _maxFillingsInclusive;

    private readonly LevelSelector _levelSelector;
    private readonly GameplayTutorialState _tutorialState;
    
    public RecipeGenerator(RecipeGeneratorSettings settings, LevelSelector levelSelector, GameplayTutorialState tutorialState)
    {
        _levelSelector = levelSelector;
        _tutorialState = tutorialState;

        var minFillings = Mathf.Max(0, settings.MinFillings);
        var maxFillingsInclusive = Mathf.Max(0, settings.MaxFillingsInclusive);

        _minFillings = Mathf.Min(minFillings, maxFillingsInclusive);
        _maxFillingsInclusive = Mathf.Max(minFillings, maxFillingsInclusive);
    }

    public RecipeGenerationResult Generate()
    {
        if (_tutorialState.IsActive)
            return new RecipeGenerationResult(_tutorialState.TutorialRecipe, false, null);

        var level = _levelSelector.SelectedLevel;
        if (level == null)
            throw new InvalidOperationException($"{nameof(RecipeGenerator)} requires a selected {nameof(Level)}.");

        var loadout = _levelSelector.GetSelectedLevelLoadout();
        if (loadout == null)
            throw new InvalidOperationException($"{nameof(RecipeGenerator)} requires a selected {nameof(Loadout)}.");

        if (level.PreferredDoughs.Count == 0)
            throw new InvalidOperationException($"Selected level '{level.name}' has no preferred doughs configured.");

        var dough = level.PreferredDoughs.GetRandomElement();
        var fillings = new Dictionary<Filling, int>();
        var fillingsCount = GetFillingsCount();
        var missingIngredients = new List<Ingredient>();
        if (!loadout.Doughs.Contains(dough))
            missingIngredients.Add(dough);

        AddFillings(fillings, fillingsCount, level, loadout, missingIngredients);

        if (missingIngredients.Count > 0)
            return FailedGeneration(missingIngredients.ToArray());

        return new RecipeGenerationResult(new Recipe(dough, fillings), false, null);
    }
    
    private int GetFillingsCount() => Random.Range(_minFillings, _maxFillingsInclusive + 1);

    private void AddFillings(
        Dictionary<Filling, int> fillings,
        int fillingsCount,
        Level level,
        Loadout loadout,
        ICollection<Ingredient> missingIngredients)
    {
        if (fillingsCount <= 0)
            return;

        var preferredEntryCount = level.PreferredFillings.Count + level.PreferredFillingTags.Count;
        if (preferredEntryCount <= 0)
            return;

        for (var i = 0; i < fillingsCount; i++)
        {
            var resolved = TryGetRandomFilling(level, loadout, out var filling, out var missingIngredient);
            if (missingIngredient != null)
                missingIngredients.Add(missingIngredient);

            if (!resolved || filling == null)
                continue;

            fillings[filling] = fillings.GetValueOrDefault(filling) + 1;
        }
    }

    private static bool TryGetRandomFilling(Level level, Loadout loadout, out Filling filling, out Ingredient missingIngredient)
    {
        var preferredFillingsCount = level.PreferredFillings.Count;
        var preferredFillingTagsCount = level.PreferredFillingTags.Count;
        var preferredEntryCount = preferredFillingsCount + preferredFillingTagsCount;
        if (preferredEntryCount <= 0)
        {
            filling = null;
            missingIngredient = null;
            return false;
        }

        var selectedIndex = Random.Range(0, preferredEntryCount);

        if (selectedIndex < preferredFillingsCount)
        {
            filling = level.PreferredFillings[selectedIndex];
            missingIngredient = loadout.Fillings.Contains(filling) ? null : filling;
            return missingIngredient == null;
        }

        var selectedTag = level.PreferredFillingTags[selectedIndex - preferredFillingsCount];
        var taggedFillings = loadout.Fillings.Where(candidate => candidate != null && candidate.HasTag(selectedTag)).ToArray();

        if (taggedFillings.Length == 0)
        {
            filling = null;
            missingIngredient = null;
            return false;
        }

        filling = taggedFillings.GetRandomElement();
        missingIngredient = null;
        return true;
    }

    private static RecipeGenerationResult FailedGeneration(params Ingredient[] missingIngredients)
    {
        return new RecipeGenerationResult(null, true, missingIngredients);
    }
}
