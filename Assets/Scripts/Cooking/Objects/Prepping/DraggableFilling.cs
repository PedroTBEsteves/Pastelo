using KBCore.Refs;
using UnityEngine;

public class DraggableFilling : DraggableIngredient<Filling>, IDiscardPolicy
{
    [SerializeField, Self]
    private SpriteRenderer _spriteRenderer;
    
    private bool _addedToPastel;
    private OpenPastelDoughArea _openPastelDoughArea;

    protected override void Awake()
    {
        base.Awake();
        
        Draggable.AddCanDragHandler(CanDrag);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        Draggable.RemoveCanDragHandler(CanDrag);
    }

    protected override bool TryAddToOpenDough(OpenPastelDoughArea openPastelDoughArea)
    {
        _addedToPastel = openPastelDoughArea.TryAddFilling(this);
        
        if (!_addedToPastel)
            Destroy(gameObject);

        _openPastelDoughArea = openPastelDoughArea;
        
        return _addedToPastel;
    }

    protected override void OnDropped(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (!_addedToPastel)
        {
            base.OnDropped(eventData);
            return;
        }

        if (_openPastelDoughArea == null)
            return;

        _openPastelDoughArea.TryRepositionFilling(this, GetMouseWorldPosition(eventData));
    }

    public bool CanBeDiscarded() => !_addedToPastel;

    public void SetSortingOrder(int sortingOrder)
    {
        _spriteRenderer.sortingOrder = sortingOrder;
    }

    private bool CanDrag() => true;
}
