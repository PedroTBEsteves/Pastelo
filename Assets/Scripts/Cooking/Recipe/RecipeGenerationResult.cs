using System;
using System.Collections.Generic;

public sealed class RecipeGenerationResult
{
    private static readonly IReadOnlyList<Ingredient> EmptyMissingIngredients = Array.Empty<Ingredient>();

    public RecipeGenerationResult(Recipe recipe, bool failedBecauseOfMissingLoadoutIngredients, IReadOnlyList<Ingredient> missingIngredients)
    {
        Recipe = recipe;
        FailedBecauseOfMissingLoadoutIngredients = failedBecauseOfMissingLoadoutIngredients;
        MissingIngredients = missingIngredients ?? EmptyMissingIngredients;
    }

    public Recipe Recipe { get; }
    public bool FailedBecauseOfMissingLoadoutIngredients { get; }
    public IReadOnlyList<Ingredient> MissingIngredients { get; }
}
