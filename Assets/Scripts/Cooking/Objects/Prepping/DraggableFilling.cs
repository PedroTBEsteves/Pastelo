using KBCore.Refs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class DraggableFilling : DraggableIngredient<Filling>, IDiscardPolicy
{
    [SerializeField, Self]
    [FormerlySerializedAs("_spriteRenderer")]
    private SpriteRenderer _fillingSpriteRenderer;
    
    private bool _addedToPastel;
    private Vector3 _slotPosition;
    private OpenPastelDoughArea _openPastelDoughArea;

    protected override void Awake()
    {
        base.Awake();
        RefreshMaskInteraction();
        
        Draggable.Held += OnHeld;
        Draggable.AddCanDragHandler(CanDrag);
    }

    protected override void OnDestroy()
    {
        if (_addedToPastel && _openPastelDoughArea != null)
            _openPastelDoughArea.RemoveFilling(this);

        base.OnDestroy();
        
        Draggable.Held -= OnHeld;
        Draggable.RemoveCanDragHandler(CanDrag);
    }

    protected override bool TryAddToOpenDough(OpenPastelDoughArea openPastelDoughArea)
    {
        _addedToPastel = openPastelDoughArea.TryAddFilling(this);
        RefreshMaskInteraction();
        
        if (!_addedToPastel)
            Destroy(gameObject);

        _openPastelDoughArea = openPastelDoughArea;
        
        return _addedToPastel;
    }

    protected override void OnDropped(PointerEventData eventData)
    {
        if (!_addedToPastel)
        {
            base.OnDropped(eventData);
            RefreshMaskInteraction();
            return;
        }

        if (_openPastelDoughArea == null)
        {
            RefreshMaskInteraction();
            return;
        }

        _openPastelDoughArea.TryRepositionFilling(this, GetMouseWorldPosition(eventData));
        RefreshMaskInteraction();
    }

    public bool CanBeDiscarded() => true;

    public void SetSortingOrder(int sortingOrder)
    {
        _fillingSpriteRenderer.sortingOrder = sortingOrder;
    }

    public Vector3 GetSlotPosition() => _slotPosition;

    public void SetSlotPosition(Vector3 slotPosition)
    {
        _slotPosition = slotPosition;
    }

    private void OnHeld(PointerEventData eventData)
    {
        RefreshMaskInteraction();
    }

    private void RefreshMaskInteraction()
    {
        _fillingSpriteRenderer.maskInteraction = _addedToPastel && !Draggable.IsDragging
            ? SpriteMaskInteraction.VisibleOutsideMask
            : SpriteMaskInteraction.None;
    }

    private bool CanDrag() => true;
}
