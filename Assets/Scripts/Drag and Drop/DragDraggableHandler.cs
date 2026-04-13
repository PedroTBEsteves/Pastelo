using UnityEngine.EventSystems;

public sealed class DragDraggableHandler : IDraggableHandler
{
    private readonly Draggable _draggable;

    public DragDraggableHandler(Draggable draggable)
    {
        _draggable = draggable;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_draggable.CanDrag())
            return;

        _draggable.BeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_draggable.CanDrag())
            return;

        _draggable.UpdateDragPosition(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_draggable.CanDrag())
            return;

        _draggable.EndDrag(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
    }
}
