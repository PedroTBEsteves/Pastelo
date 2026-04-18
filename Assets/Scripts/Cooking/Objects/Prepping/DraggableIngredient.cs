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

    private SpriteRenderer _spriteRenderer;

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

        TryAddToOpenDough(openPastelDoughArea);
    }

    protected Vector3 GetMouseWorldPosition(PointerEventData eventData)
    {
        return _cameraController.ScreenToWorldPoint(eventData.position);
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

    public void Configure(TIngredient ingredient)
    {
        Ingredient = ingredient;

        if (Ingredient == null)
            return;

        _spriteRenderer ??= GetComponent<SpriteRenderer>();

        if (_spriteRenderer != null)
            _spriteRenderer.sprite = Ingredient.Icon;
    }

    protected abstract bool TryAddToOpenDough(OpenPastelDoughArea openPastelDoughArea);
}
