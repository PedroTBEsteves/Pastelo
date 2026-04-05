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

    protected virtual void OnDropped(PointerEventData eventData)
    {
        if (!TryGetOpenPastelDoughArea(eventData, out var openPastelDoughArea))
        {
            Destroy(gameObject);
            return;
        }

        if (!CanSpend())
        {
            Destroy(gameObject);
            ShowNotEnoughMoneyError();
            return;
        }

        if (TryAddToOpenDough(openPastelDoughArea))
        {
            Spend();
        }
    }

    protected Vector3 GetMouseWorldPosition(PointerEventData eventData)
    {
        return _cameraController.ScreenToWorldPointy(eventData.position);
    }

    protected bool TryGetOpenPastelDoughArea(PointerEventData eventData, out OpenPastelDoughArea openPastelDoughArea)
    {
        var mousePosition = GetMouseWorldPosition(eventData);
        var raycastHit = Physics2D.Raycast(
            mousePosition,
            Vector2.zero,
            float.MaxValue,
            ~LayerMask.GetMask("Draggable", "Ignore Raycast"));

        if (raycastHit && raycastHit.collider.TryGetComponent(out openPastelDoughArea))
            return true;

        openPastelDoughArea = null;
        return false;
    }

    protected bool CanSpend() => _money.CanSpend(Ingredient.Cost);

    protected void Spend()
    {
        _money.TrySpend(Ingredient.Cost);
    }

    protected void ShowNotEnoughMoneyError()
    {
        _popupTextService.ShowError("Sem dinheiro suficiente!", transform.position);
    }

    protected abstract bool TryAddToOpenDough(OpenPastelDoughArea openPastelDoughArea);
}
