using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public sealed class DraggableUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private DraggableInputConfiguration _inputConfiguration;
    private bool _isDragging;
    private bool _createdDragOnCurrentPress;
    private bool _pendingPointerClickIgnore;
    private bool _ignoreNextPointerClick;

    private readonly List<Func<bool>> _canDragHandlers = new();

    public event Action<PointerEventData> Held = delegate { };
    public event Action<Vector2> Dragged = delegate { };
    public event Action<PointerEventData> Dropped = delegate { };

    public bool IsDragging => _isDragging;

    private bool IsClickInputMode => _inputConfiguration != null && _inputConfiguration.Mode == DraggableInputMode.Click;

    private void Update()
    {
        if (!_isDragging || !IsClickInputMode || Pointer.current == null)
            return;

        Dragged(Pointer.current.position.ReadValue());
    }

    public void Configure(DraggableInputConfiguration inputConfiguration)
    {
        _inputConfiguration = inputConfiguration;
    }

    public void AddCanDragHandler(Func<bool> handler) => _canDragHandlers.Add(handler);

    public void RemoveCanDragHandler(Func<bool> handler) => _canDragHandlers.Remove(handler);

    public void BeginExternalDrag(PointerEventData eventData, bool notifyHeld = false, bool ignoreNextEligibleClick = false)
    {
        if (!CanDrag())
            return;

        BeginDrag(eventData, notifyHeld, ignoreNextEligibleClick);
    }

    public void CancelDrag()
    {
        _isDragging = false;
        _createdDragOnCurrentPress = false;
        _pendingPointerClickIgnore = false;
        _ignoreNextPointerClick = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!EnsureConfigured() || !IsClickInputMode || _isDragging || !CanDrag())
            return;

        BeginDrag(eventData, notifyHeld: true, ignoreNextEligibleClick: true);
        _createdDragOnCurrentPress = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!EnsureConfigured() || !IsClickInputMode || !_createdDragOnCurrentPress)
            return;

        _createdDragOnCurrentPress = false;
        FinalizePendingPointerClickIgnore(eventData.eligibleForClick);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!EnsureConfigured() || IsClickInputMode || !CanDrag())
            return;

        BeginDrag(eventData, notifyHeld: true, ignoreNextEligibleClick: false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!EnsureConfigured() || !_isDragging)
            return;

        Dragged(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!EnsureConfigured() || IsClickInputMode || !_isDragging)
            return;

        EndDrag(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!EnsureConfigured() || !IsClickInputMode)
            return;

        if (TryConsumeIgnoredPointerClick())
            return;

        if (_isDragging)
            EndDrag(eventData);
    }

    private void BeginDrag(PointerEventData eventData, bool notifyHeld, bool ignoreNextEligibleClick)
    {
        _isDragging = true;
        _pendingPointerClickIgnore = ignoreNextEligibleClick;
        _ignoreNextPointerClick = false;

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

    private void FinalizePendingPointerClickIgnore(bool shouldIgnore)
    {
        _ignoreNextPointerClick = _pendingPointerClickIgnore && shouldIgnore;
        _pendingPointerClickIgnore = false;
    }

    private bool TryConsumeIgnoredPointerClick()
    {
        if (!_ignoreNextPointerClick)
            return false;

        _ignoreNextPointerClick = false;
        return true;
    }

    private bool EnsureConfigured()
    {
        if (_inputConfiguration != null)
            return true;

        Debug.LogError($"{nameof(DraggableUI)} on '{name}' needs {nameof(DraggableInputConfiguration)} configured before handling input.", this);
        return false;
    }
}
