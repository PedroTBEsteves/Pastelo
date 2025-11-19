using System;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(DraggableSink))]
public abstract class DraggableIngredientSink<TIngredient> : ValidatedMonoBehaviour, IPointerDownHandler where TIngredient : Ingredient
{
    [SerializeField] 
    private TIngredient _ingredient;
    
    [SerializeField, Child(Flag.ExcludeSelf)]
    private Transform _lockedIndicator;
    
    [SerializeField, Self]
    private DraggableSink _draggableSink;
    
    [Inject]
    private readonly Money _money;
    
    [Inject]
    private readonly IngredientsStorage _ingredientsStorage;
    
    [Inject]
    private readonly IPopupTextService _popupTextService;

    private void Awake()
    {
        var isUnlocked = _ingredientsStorage.Contains(_ingredient);
        
        _lockedIndicator.gameObject.SetActive(!isUnlocked);
        
        _draggableSink.AddCanCreateDraggableHandler(CanCreateDraggable);
    }

    private void OnDestroy()
    {
        _draggableSink.RemoveCanCreateDraggableHandler(CanCreateDraggable);
    }

    private bool CanCreateDraggable()
    {
        return _ingredientsStorage.Contains(_ingredient);
    }

    public void OnPointerDown(PointerEventData eventData)
    { ;
        var isUnlocked = _ingredientsStorage.Contains(_ingredient);

        if (isUnlocked)
            return;
        
        if (_ingredientsStorage.TryBuyIngredient(_ingredient))
        {
            _lockedIndicator.gameObject.SetActive(false);
        }
        else
        {
            _popupTextService.ShowError("Sem dinheiro suficiente!", transform.position);
        }
    }
}
