using System;
using System.Collections.Generic;
using System.Linq;
using Reflex.Attributes;
using UnityEngine;

public sealed class LevelLoadoutLoader : MonoBehaviour
{
    [SerializeField]
    private DraggableDoughSource[] _doughSources;

    [SerializeField]
    private DraggableFillingSource[] _fillingSources;

    [Inject]
    private readonly LevelSelector _levelSelector;

    private void Awake()
    {
        var loadout = _levelSelector.GetSelectedLevelLoadout();
        ConfigureSources(GetOrderedIngredients(loadout.Doughs), _doughSources);
        ConfigureSources(GetOrderedIngredients(loadout.Fillings), _fillingSources);
    }

    private static IReadOnlyList<TIngredient> GetOrderedIngredients<TIngredient>(IEnumerable<TIngredient> ingredients)
        where TIngredient : Ingredient
    {
        return ingredients
            .Where(ingredient => ingredient != null)
            .OrderBy(ingredient => ingredient.InternalName, StringComparer.Ordinal)
            .ThenBy(ingredient => ingredient.name, StringComparer.Ordinal)
            .ToArray();
    }

    private void ConfigureSources<TIngredient, TSource>(IReadOnlyList<TIngredient> ingredients, IReadOnlyList<TSource> sources)
        where TIngredient : Ingredient
        where TSource : DraggableIngredientSource<TIngredient>
    {
        if (sources == null)
            return;

        var orderedSources = sources.Where(source => source != null).ToArray();
        var sourceCount = orderedSources.Length;

        if (ingredients.Count > sourceCount)
        {
            Debug.LogWarning(
                $"{nameof(LevelLoadoutLoader)} has {ingredients.Count} configured {typeof(TIngredient).Name} ingredients but only {sourceCount} sources available on '{name}'.",
                this);
        }

        for (var i = 0; i < sourceCount; i++)
        {
            var source = orderedSources[i];

            if (i < ingredients.Count)
            {
                source.Configure(ingredients[i]);
                continue;
            }

            source.gameObject.SetActive(false);
            Destroy(source.gameObject);
        }
    }
}
