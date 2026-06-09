using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public sealed class DraggableUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private enum InteractionGesture
    {
        None,
        Pending,
        Drag,
        Click
    }
    
    private bool _isDragging;
    private bool _createdDragOnCurrentPress;
    private InteractionGesture _activeGesture;

    private readonly List<Func<bool>> _canDragHandlers = new();

    public event Action<PointerEventData> Held = delegate { };
    public event Action<Vector2> Dragged = delegate { };
    public event Action<PointerEventData> Dropped = delegate { };

    public bool IsDragging => _isDragging;
    public bool IsUsingClickGesture => _activeGesture == InteractionGesture.Click;

    private void Update()
    {
        if (!_isDragging || _activeGesture != InteractionGesture.Click || Pointer.current == null)
            return;

        Dragged(Pointer.current.position.ReadValue());
    }

    public void AddCanDragHandler(Func<bool> handler) => _canDragHandlers.Add(handler);

    public void RemoveCanDragHandler(Func<bool> handler) => _canDragHandlers.Remove(handler);

    public void BeginExternalDrag(PointerEventData eventData, bool notifyHeld = false, bool ignoreNextEligibleClick = false)
    {
        if (!CanDrag())
            return;

        BeginDrag(eventData, InteractionGesture.Click, notifyHeld);
    }

    public void CancelDrag()
    {
        _isDragging = false;
        _createdDragOnCurrentPress = false;
        _activeGesture = InteractionGesture.None;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_isDragging || !CanDrag())
            return;

        _activeGesture = InteractionGesture.Pending;
        _createdDragOnCurrentPress = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_isDragging || _activeGesture != InteractionGesture.Pending || !CanDrag())
            return;

        _createdDragOnCurrentPress = false;
        BeginDrag(eventData, InteractionGesture.Drag, notifyHeld: true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging)
            return;

        Dragged(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging || _activeGesture != InteractionGesture.Drag)
            return;

        EndDrag(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isDragging)
        {
            if (_activeGesture == InteractionGesture.Click)
                EndDrag(eventData);
            return;
        }

        if (_activeGesture != InteractionGesture.Pending || !_createdDragOnCurrentPress)
            return;

        _createdDragOnCurrentPress = false;
        BeginDrag(eventData, InteractionGesture.Click, notifyHeld: true);
    }

    private void BeginDrag(PointerEventData eventData, InteractionGesture gesture, bool notifyHeld)
    {
        _isDragging = true;
        _activeGesture = gesture;

        if (notifyHeld)
            Held(eventData);

        Dragged(eventData.position);
    }

    private void EndDrag(PointerEventData eventData)
    {
        CancelDrag();
        Dropped(eventData);
    }

    private bool CanDrag() => _canDragHandlers.Aggregate(true, (agg, handler) => agg && handler());
}
