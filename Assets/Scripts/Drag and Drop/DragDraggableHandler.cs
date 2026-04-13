using UnityEngine.EventSystems;

public sealed class DragDraggableHandler : IDraggableHandler
{
    public void OnBeginDrag(Draggable draggable, PointerEventData eventData)
    {
        if (!draggable.CanDrag())
            return;

        draggable.BeginDrag(eventData);
    }

    public void OnDrag(Draggable draggable, PointerEventData eventData)
    {
        if (!draggable.CanDrag())
            return;

        draggable.UpdateDragPosition(eventData.position);
    }

    public void OnEndDrag(Draggable draggable, PointerEventData eventData)
    {
        if (!draggable.IsDragging)
            return;

        draggable.EndDrag(eventData);
    }

    public void OnPointerClick(Draggable draggable, PointerEventData eventData)
    {
        if (!draggable.IsDragging)
            return;

        draggable.EndDrag(eventData);
    }
}
