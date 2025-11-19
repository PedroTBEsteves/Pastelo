using System;
using System.Collections.Generic;
using System.Linq;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class DraggableSink : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
    private Draggable _draggablePrefab;

    [Inject]
    private readonly CameraController _cameraController;

    private Draggable _draggable;

    private readonly List<Func<bool>> _canCreateDraggableHandlers = new();
    
    public void AddCanCreateDraggableHandler(Func<bool> handler) => _canCreateDraggableHandlers.Add(handler);
    
    public void RemoveCanCreateDraggableHandler(Func<bool> handler) =>  _canCreateDraggableHandlers.Remove(handler);
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanCreateDraggable())
            return;
        
        var position = eventData.pointerCurrentRaycast.worldPosition;
        
        _draggable = Instantiate(_draggablePrefab, position, Quaternion.identity);
        
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
    
    private bool CanCreateDraggable() => _canCreateDraggableHandlers.Aggregate(true, (agg, handler) => agg && handler());
}
