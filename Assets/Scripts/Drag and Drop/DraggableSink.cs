using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class DraggableSink<TDraggable> : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler where TDraggable : Draggable
{
    [SerializeField]
    protected TDraggable DraggablePrefab;

    [Inject]
    private readonly CameraController _cameraController;

    private Draggable _draggable;
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanCreateDraggable())
            return;
        
        var position = eventData.pointerCurrentRaycast.worldPosition;
        
        _draggable = Instantiate(DraggablePrefab, position, Quaternion.identity);
        
        ExecuteEvents.Execute(_draggable.gameObject, eventData, ExecuteEvents.beginDragHandler);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_draggable == null)
            return;
        
        ExecuteEvents.Execute(_draggable.gameObject, eventData, ExecuteEvents.dragHandler);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_draggable == null)
            return;
        
        ExecuteEvents.Execute(_draggable.gameObject, eventData, ExecuteEvents.endDragHandler);
        _draggable = null;
    }
    
    protected virtual bool CanCreateDraggable() => true;
}
