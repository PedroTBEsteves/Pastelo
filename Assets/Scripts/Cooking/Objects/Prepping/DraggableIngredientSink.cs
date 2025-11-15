using System;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class DraggableIngredientSink<TIngredient> : DraggableSink<DraggableIngredient<TIngredient>>, IPointerDownHandler where TIngredient : Ingredient
{
    [Inject]
    private readonly Money _money;
    
    [Inject]
    private readonly IngredientsStorage _ingredientsStorage;
    
    [Inject]
    private readonly IPopupTextService _popupTextService;

    [SerializeField, Child(Flag.ExcludeSelf)]
    private Transform _lockedIndicator;

    private void Awake()
    {
        var ingredient = DraggablePrefab.Ingredient;
        var isUnlocked = _ingredientsStorage.Contains(ingredient);
        
        _lockedIndicator.gameObject.SetActive(!isUnlocked);
    }

    protected override bool CanCreateDraggable()
    {
        return _ingredientsStorage.Contains(DraggablePrefab.Ingredient);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        var ingredient = DraggablePrefab.Ingredient;
        var isUnlocked = _ingredientsStorage.Contains(ingredient);

        if (isUnlocked)
            return;
        
        if (_ingredientsStorage.TryBuyIngredient(ingredient))
        {
            _lockedIndicator.gameObject.SetActive(false);
        }
        else
        {
            _popupTextService.ShowError("Sem dinheiro suficiente!", transform.position);
        }
    }
}
