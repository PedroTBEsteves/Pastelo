using UnityEngine;
using UnityEngine.EventSystems;

public interface IDraggableHandler
{
    void OnPointerDown(Draggable draggable, PointerEventData eventData);
    void OnPointerUp(Draggable draggable, PointerEventData eventData);
    void OnBeginDrag(Draggable draggable, PointerEventData eventData);
    void OnDrag(Draggable draggable, PointerEventData eventData);
    void OnEndDrag(Draggable draggable, PointerEventData eventData);
    void OnPointerClick(Draggable draggable, PointerEventData eventData);
}
