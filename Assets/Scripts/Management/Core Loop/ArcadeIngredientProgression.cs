using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class ArcadeIngredientProgression : IDisposable
{
    private readonly LevelRunContext _runContext;
    private readonly OrderController _orderController;
    private readonly List<Ingredient> _unlockPool;

    public ArcadeIngredientProgression(
        LevelRunContext runContext,
        OrderController orderController,
        IngredientsStorageSettings ingredientsStorageSettings)
    {
        _runContext = runContext ?? throw new ArgumentNullException(nameof(runContext));
        _orderController = orderController ?? throw new ArgumentNullException(nameof(orderController));

        if (ingredientsStorageSettings == null)
            throw new ArgumentNullException(nameof(ingredientsStorageSettings));

        _unlockPool = ingredientsStorageSettings.Ingredients
            .Where(ingredient => ingredient != null)
            .Distinct()
            .ToList();

        RemoveUnlockedIngredientsFromPool();
        _orderController.OrderSucceeded += OnOrderSucceeded;
    }

    public void Dispose()
    {
        _orderController.OrderSucceeded -= OnOrderSucceeded;
    }

    private void OnOrderSucceeded(Order _)
    {
        if (!_runContext.IsArcade)
            return;

        RemoveUnlockedIngredientsFromPool();

        var candidates = _unlockPool
            .Where(HasCapacityForIngredient)
            .ToArray();

        if (candidates.Length == 0)
            return;

        var ingredient = candidates[UnityEngine.Random.Range(0, candidates.Length)];
        _unlockPool.Remove(ingredient);

        var added = ingredient switch
        {
            Dough dough => _runContext.CurrentLoadout.AddDough(dough),
            Filling filling => _runContext.CurrentLoadout.AddFilling(filling),
            _ => false
        };

        if (!added)
            return;

        Debug.Log($"Arcade unlocked ingredient '{ingredient.name}'.");
        _runContext.NotifyLoadoutChanged();
    }

    private void RemoveUnlockedIngredientsFromPool()
    {
        if (!_runContext.HasActiveRun)
            return;

        var loadout = _runContext.CurrentLoadout;
        _unlockPool.RemoveAll(ingredient => ingredient switch
        {
            Dough dough => loadout.Doughs.Contains(dough),
            Filling filling => loadout.Fillings.Contains(filling),
            _ => true
        });
    }

    private bool HasCapacityForIngredient(Ingredient ingredient)
    {
        var loadout = _runContext.CurrentLoadout;
        return ingredient switch
        {
            Dough => loadout.DoughCount < loadout.MaxDoughs,
            Filling => loadout.FillingCount < loadout.MaxFillings,
            _ => false
        };
    }
}
