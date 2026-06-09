using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class DraggableSource : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [SerializeField]
    private Draggable _draggablePrefab;

    private Draggable _draggable;
    private bool _hasPendingPress;

    private readonly List<Func<bool>> _canCreateDraggableHandlers = new();
    private readonly List<Action<Draggable>> _draggableCreatedHandlers = new();
    
    public void AddCanCreateDraggableHandler(Func<bool> handler) => _canCreateDraggableHandlers.Add(handler);
    
    public void RemoveCanCreateDraggableHandler(Func<bool> handler) =>  _canCreateDraggableHandlers.Remove(handler);

    public void AddDraggableCreatedHandler(Action<Draggable> handler) => _draggableCreatedHandlers.Add(handler);

    public void RemoveDraggableCreatedHandler(Action<Draggable> handler) => _draggableCreatedHandlers.Remove(handler);

    public void Configure(Draggable draggablePrefab)
    {
        _draggablePrefab = draggablePrefab;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_draggable != null)
            return;

        if (!CanCreateDraggable())
            return;

        _hasPendingPress = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_hasPendingPress || _draggable != null)
            return;

        if (!CanCreateDraggable())
            return;
        
        _draggable = CreateDraggable(eventData);
        if (_draggable == null)
            return;

        _hasPendingPress = false;
        _draggable.BeginPointerDrag(eventData);

        if (!_draggable.IsDragging)
            ClearCurrentDraggable(_draggable);
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
        _hasPendingPress = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_draggable != null || !_hasPendingPress)
            return;

        if (!CanCreateDraggable())
            return;

        _draggable = CreateDraggable(eventData);
        if (_draggable == null)
            return;

        _hasPendingPress = false;
        _draggable.BeginClickDrag(eventData);

        if (!_draggable.IsDragging)
            ClearCurrentDraggable(_draggable);
    }
    
    private bool CanCreateDraggable() => _canCreateDraggableHandlers.Aggregate(true, (agg, handler) => agg && handler());

    private Draggable CreateDraggable(PointerEventData eventData)
    {
        if (_draggablePrefab == null)
        {
            Debug.LogError($"{nameof(DraggableSource)} on '{name}' is missing a draggable prefab.", this);
            return null;
        }

        var position = eventData.pointerCurrentRaycast.worldPosition;
        var draggable = Instantiate(_draggablePrefab, position, Quaternion.identity);
        draggable.Dropped += OnDraggableDropped;

        foreach (var handler in _draggableCreatedHandlers)
            handler?.Invoke(draggable);

        return draggable;
    }

    private void OnDraggableDropped(PointerEventData eventData)
    {
        _hasPendingPress = false;

        if (_draggable == null)
            return;

        ClearCurrentDraggable(_draggable);
    }

    private void ClearCurrentDraggable(Draggable draggable)
    {
        if (draggable == null)
            return;

        draggable.Dropped -= OnDraggableDropped;

        if (_draggable == draggable)
            _draggable = null;
    }
}
