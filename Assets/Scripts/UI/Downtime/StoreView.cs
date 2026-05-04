using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

public class StoreView : MonoBehaviour
{
    [SerializeField]
    private Transform _randomIngredientsRoot;

    [SerializeField]
    [FormerlySerializedAs("_randomIngredientPrefab")]
    private IngredientStoreView _ingredientPrefab;

    [SerializeField]
    private Transform _fixedIngredientsRoot;

    [SerializeField]
    private IngredientStorePurchasePrompt _purchasePrompt;

    [Inject]
    private readonly Store _store;

    private void Awake()
    {
        BuildRandomIngredients();
        BuildFixedIngredients();
    }

    private void BuildRandomIngredients()
    {
        var randomIngredients = _store.RandomIngredients;

        foreach (var ingredient in randomIngredients)
        {
            var itemView = Instantiate(_ingredientPrefab, _randomIngredientsRoot);
            itemView.BindDaily(ingredient, _purchasePrompt);
        }
    }

    private void BuildFixedIngredients()
    {
        var fixedIngredients = _store.FixedIngredients;

        foreach (var fixedIngredientOffer in fixedIngredients)
        {
            var itemView = Instantiate(_ingredientPrefab, _fixedIngredientsRoot);
            itemView.BindFixed(fixedIngredientOffer, _purchasePrompt);
        }
    }
}
