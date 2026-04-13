using UnityEngine.EventSystems;

public sealed class ClickDraggableHandler : IDraggableHandler
{
    private readonly Draggable _draggable;

    public ClickDraggableHandler(Draggable draggable)
    {
        _draggable = draggable;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_draggable.IsDragging)
            return;

        _draggable.UpdateDragPosition(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_draggable.IsDragging)
        {
            _draggable.EndDrag(eventData);
            return;
        }

        if (!_draggable.CanDrag())
            return;

        _draggable.BeginDrag(eventData, followPointerContinuously: true);
        _draggable.UpdateDragPosition(eventData.position);
    }
}
