using System;
using System.Collections.Generic;
using System.Linq;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class DraggableSource : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [SerializeField]
    private Draggable _draggablePrefab;

    [Inject]
    private readonly DraggableInputConfiguration _inputConfiguration;

    private Draggable _draggable;
    private bool _createdDraggableOnCurrentPress;

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
        if (_inputConfiguration.Mode != DraggableInputMode.Click)
            return;

        if (_draggable != null)
            return;

        if (!CanCreateDraggable())
            return;

        _draggable = CreateDraggable(eventData);
        if (_draggable == null)
            return;

        _draggable.Dropped += OnDraggableDropped;
        _createdDraggableOnCurrentPress = true;
        ExecuteEvents.Execute(_draggable.gameObject, eventData, ExecuteEvents.pointerDownHandler);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_inputConfiguration.Mode != DraggableInputMode.Click)
            return;

        if (!_createdDraggableOnCurrentPress || _draggable == null)
            return;

        _createdDraggableOnCurrentPress = false;
        _draggable.FinalizePendingPointerClickIgnore(eventData.eligibleForClick);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_inputConfiguration.Mode != DraggableInputMode.Drag)
            return;

        if (!CanCreateDraggable())
            return;
        
        _draggable = CreateDraggable(eventData);
        if (_draggable == null)
            return;
        
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

        if (_draggable == null)
            return;

        var currentDraggable = _draggable;

        ExecuteEvents.Execute(currentDraggable.gameObject, eventData, ExecuteEvents.pointerClickHandler);

        if (_draggable == currentDraggable && !currentDraggable.IsDragging)
            ClearCurrentDraggable(currentDraggable);
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

        foreach (var handler in _draggableCreatedHandlers)
            handler?.Invoke(draggable);

        return draggable;
    }

    private void OnDraggableDropped(PointerEventData eventData)
    {
        _createdDraggableOnCurrentPress = false;

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
