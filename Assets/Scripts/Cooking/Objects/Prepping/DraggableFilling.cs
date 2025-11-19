using KBCore.Refs;
using UnityEngine;

public class DraggableFilling : DraggableIngredient<Filling>
{
    [SerializeField, Self]
    private SpriteRenderer _spriteRenderer;
    
    private bool _addedToPastel;

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

        _spriteRenderer.sortingOrder = openPastelDoughArea.FillingsCount;
        
        return _addedToPastel;
    }

    private bool CanDrag() => !_addedToPastel;
}
