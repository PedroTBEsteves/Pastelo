using UnityEngine.EventSystems;

public sealed class ClickDraggableHandler : IDraggableHandler
{
    public void OnBeginDrag(Draggable draggable, PointerEventData eventData)
    {
    }

    public void OnDrag(Draggable draggable, PointerEventData eventData)
    {
        if (!draggable.IsDragging)
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
        if (draggable.IsDragging)
        {
            draggable.EndDrag(eventData);
            return;
        }

        if (!draggable.CanDrag())
            return;

        draggable.BeginDrag(eventData, followPointerContinuously: true);
        draggable.UpdateDragPosition(eventData.position);
    }
}
