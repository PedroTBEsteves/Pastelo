using System;
using System.Collections.Generic;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using Object = UnityEngine.Object;

public class StoreView : ValidatedMonoBehaviour
{
    [SerializeField]
    private Transform _randomIngredientsRoot;

    [SerializeField]
    private IngredientStorePurchaseButton _randomIngredientPrefab;

    [SerializeField]
    private Transform _fixedIngredientsRoot;

    [SerializeField]
    private FixedStoreIngredientView _fixedIngredientPrefab;

    [Inject]
    private readonly Store _store;

    private void Awake()
    {
        BuildRandomIngredients();
        BuildFixedIngredients();
    }

    private void BuildRandomIngredients()
    {
        if (_randomIngredientsRoot == null || _randomIngredientPrefab == null)
            return;

        var randomIngredients = _store.RandomIngredients;

        foreach (var ingredient in randomIngredients)
        {
            var itemView = Instantiate(_randomIngredientPrefab, _randomIngredientsRoot);
            itemView.Bind(ingredient);
        }
    }

    private void BuildFixedIngredients()
    {
        if (_fixedIngredientsRoot == null || _fixedIngredientPrefab == null)
            return;

        var fixedIngredients = _store.FixedIngredients;

        foreach (var fixedIngredientOffer in fixedIngredients)
        {
            var itemView = Instantiate(_fixedIngredientPrefab, _fixedIngredientsRoot);
            itemView.Bind(fixedIngredientOffer);
        }
    }
}
