using System;
using System.Collections.Generic;
using System.Linq;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class DraggableSink : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField]
    private Draggable _draggablePrefab;

    [Inject]
    private readonly DraggableInputConfiguration _inputConfiguration;

    private Draggable _draggable;

    private readonly List<Func<bool>> _canCreateDraggableHandlers = new();
    
    public void AddCanCreateDraggableHandler(Func<bool> handler) => _canCreateDraggableHandlers.Add(handler);
    
    public void RemoveCanCreateDraggableHandler(Func<bool> handler) =>  _canCreateDraggableHandlers.Remove(handler);
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_inputConfiguration.Mode != DraggableInputMode.Drag)
            return;

        if (!CanCreateDraggable())
            return;
        
        _draggable = CreateDraggable(eventData);
        
        ExecuteEvents.Execute(_draggable.gameObject, eventData, ExecuteEvents.beginDragHandler);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_inputConfiguration.Mode != DraggableInputMode.Drag)
            return;

        if (_draggable == null)
            return;
        
        ExecuteEvents.Execute(_draggable.gameObject, eventData, ExecuteEvents.dragHandler);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_inputConfiguration.Mode != DraggableInputMode.Drag)
            return;

        if (_draggable == null)
            return;
        
        ExecuteEvents.Execute(_draggable.gameObject, eventData, ExecuteEvents.endDragHandler);
        _draggable = null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_inputConfiguration.Mode != DraggableInputMode.Click)
            return;

        if (_draggable != null && !_draggable.IsDragging)
            _draggable = null;

        if (_draggable == null)
        {
            if (!CanCreateDraggable())
                return;

            _draggable = CreateDraggable(eventData);
        }

        ExecuteEvents.Execute(_draggable.gameObject, eventData, ExecuteEvents.pointerClickHandler);

        if (!_draggable.IsDragging)
            _draggable = null;
    }
    
    private bool CanCreateDraggable() => _canCreateDraggableHandlers.Aggregate(true, (agg, handler) => agg && handler());

    private Draggable CreateDraggable(PointerEventData eventData)
    {
        var position = eventData.pointerCurrentRaycast.worldPosition;
        return Instantiate(_draggablePrefab, position, Quaternion.identity);
    }
}
