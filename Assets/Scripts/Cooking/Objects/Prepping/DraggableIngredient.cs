using System;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Draggable))]
public abstract class DraggableIngredient<TIngredient> : ValidatedMonoBehaviour where TIngredient : Ingredient
{
    [SerializeField]
    public TIngredient Ingredient;
    
    [field: SerializeField, Self]
    protected Draggable Draggable { get; private set; }

    [Inject]
    private readonly CameraController _cameraController;

    [Inject]
    private readonly Money _money;
    
    [Inject]
    private readonly IPopupTextService _popupTextService;

    protected virtual void Awake()
    {
        Draggable.Dropped += OnDropped;
    }

    protected virtual void OnDestroy()
    {
        Draggable.Dropped -= OnDropped;
    }

    private void OnDropped(PointerEventData eventData)
    {
        var mousePosition = _cameraController.ScreenToWorldPointy(eventData.position);
        var raycastHit = Physics2D.Raycast(
            mousePosition,
            Vector2.zero,
            float.MaxValue,
            ~LayerMask.GetMask("Draggable"));
        
        if (!raycastHit 
            || !raycastHit.collider.TryGetComponent<OpenPastelDoughArea>(out var openPastelDoughArea))
        {
            Destroy(gameObject);
            return;
        }

        if (!_money.CanSpend(Ingredient.Cost))
        {
            Destroy(gameObject);
            _popupTextService.ShowError("Sem dinheiro suficiente!", transform.position);
            return;
        }

        if (TryAddToOpenDough(openPastelDoughArea))
        {
            _money.TrySpend(Ingredient.Cost);
        }
    }
    
    protected abstract bool TryAddToOpenDough(OpenPastelDoughArea openPastelDoughArea);
}
