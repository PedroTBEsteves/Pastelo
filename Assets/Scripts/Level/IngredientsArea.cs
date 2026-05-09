using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class IngredientsArea<TIngredient, TSource> : MonoBehaviour
    where TIngredient : Ingredient
    where TSource : DraggableIngredientSource<TIngredient>
{
    protected abstract IReadOnlyList<TSource> Sources { get; }

    public void Configure(IEnumerable<TIngredient> ingredients)
    {
        if (ingredients == null)
            throw new ArgumentNullException(nameof(ingredients));

        var orderedIngredients = ingredients.ToList();

        if (orderedIngredients.Count > Sources.Count)
        {
            Debug.LogWarning(
                $"{GetType().Name} has {orderedIngredients.Count} configured {typeof(TIngredient).Name} ingredients but only {Sources.Count} sources available on '{name}'.",
                this);
        }

        for (var i = 0; i < Sources.Count; i++)
        {
            var source = Sources[i];
            var hasIngredient = i < orderedIngredients.Count;

            source.gameObject.SetActive(hasIngredient);

            if (hasIngredient)
                source.Configure(orderedIngredients[i]);
        }
    }
}
