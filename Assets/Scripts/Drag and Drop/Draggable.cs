using System;
using System.Collections.Generic;
using System.Linq;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public sealed class Draggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Inject]
    private readonly CameraController _cameraController;
    
    private Vector2 _holdOffset;
    
    private SpriteRenderer _sprite;
    private int _order;
    
    private bool _dragging;
    private bool _followPointerContinuously;
    private bool _transitioning;
    
    private IDraggableHandler _handler;
    
    private readonly List<Func<bool>> _canDragHandlers = new();
    
    public event Action<PointerEventData> Held = delegate { };
    public event Action<PointerEventData> Dropped = delegate { };
    
    public bool IsDragging => _dragging;
    
    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Draggable");
        _sprite = GetComponent<SpriteRenderer>();
        _order = _sprite != null ? _sprite.sortingOrder : 0;
        _handler = new ClickDraggableHandler(this);

        _cameraController.CameraBeganMoving += OnCameraTransitionStarted;
        _cameraController.CameraEndedMoving += OnCameraTransitionFinished;
    }

    private void Update()
    {
        if (!_dragging || (!_followPointerContinuously && !_transitioning))
            return;
        
        if (Pointer.current == null)
            return;

        UpdateDragPosition(Pointer.current.position.ReadValue());
    }

    private void OnDestroy()
    {
        _cameraController.CameraBeganMoving -= OnCameraTransitionStarted;
        _cameraController.CameraEndedMoving -= OnCameraTransitionFinished;
    }

    public void OnDrag(PointerEventData eventData) => _handler.OnDrag(eventData);

    public void OnBeginDrag(PointerEventData eventData) => _handler.OnBeginDrag(eventData);

    public void OnEndDrag(PointerEventData eventData) => _handler.OnEndDrag(eventData);
    
    public void OnPointerClick(PointerEventData eventData) => _handler.OnPointerClick(eventData);
    
    public void AddCanDragHandler(Func<bool> handler) => _canDragHandlers.Add(handler);
    
    public void RemoveCanDragHandler(Func<bool> handler) =>  _canDragHandlers.Remove(handler);
    
    public bool CanDrag() => _canDragHandlers.Aggregate(true, (agg, handler) => agg && handler());
    
    public void BeginDrag(PointerEventData eventData, bool followPointerContinuously = false)
    {
        if (!CanDrag())
            return;

        if (_sprite != null)
            _sprite.sortingOrder = 9;
        
        _holdOffset = transform.position - eventData.pointerCurrentRaycast.worldPosition;
        _followPointerContinuously = followPointerContinuously;
        Held(eventData);
        _dragging = true;
    }

    public void UpdateDragPosition(Vector2 screenPosition)
    {
        if (!_dragging)
            return;

        transform.position = _cameraController.ScreenToWorldPoint(screenPosition) + _holdOffset;
    }

    public void EndDrag(PointerEventData eventData)
    {
        if (!_dragging)
            return;
        
        if (_sprite != null)
            _sprite.sortingOrder = _order;
        
        _holdOffset = Vector2.zero;
        _followPointerContinuously = false;
        Dropped(eventData);
        _dragging = false;
    }

    private void OnCameraTransitionStarted()
    {
        if (_dragging)
        {
            _transitioning = true;
        }
    }

    private void OnCameraTransitionFinished()
    {
        _transitioning = false;
    }
}
