using UnityEngine;
using UnityEngine.EventSystems;

public interface IDraggableHandler
{
    void OnBeginDrag(Draggable draggable, PointerEventData eventData);
    void OnDrag(Draggable draggable, PointerEventData eventData);
    void OnEndDrag(Draggable draggable, PointerEventData eventData);
    void OnPointerClick(Draggable draggable, PointerEventData eventData);
}
