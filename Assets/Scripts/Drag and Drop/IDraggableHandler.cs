using UnityEngine;
using UnityEngine.EventSystems;

public interface IDraggableHandler
{
    void OnBeginDrag(PointerEventData eventData);
    void OnDrag(PointerEventData eventData);
    void OnEndDrag(PointerEventData eventData);
    void OnPointerClick(PointerEventData eventData);
}
