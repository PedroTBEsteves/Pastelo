using System;
using System.Collections.Generic;
using System.Linq;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public sealed class Draggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [Inject]
    private readonly CameraController _cameraController;
    
    private Vector2 _holdOffset;
    
    private SpriteRenderer _sprite;
    private int _order;
    
    private bool _dragging;
    private bool _transitioning;
    
    private readonly List<Func<bool>> _canDragHandlers = new();
    
    public event Action<PointerEventData> Held = delegate { };
    public event Action<PointerEventData> Dropped = delegate { };
    
    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Draggable");
        _sprite = GetComponent<SpriteRenderer>();
        _order = _sprite != null ? _sprite.sortingOrder : 0;

        _cameraController.CameraBeganMoving += OnCameraTransitionStarted;
        _cameraController.CameraEndedMoving += OnCameraTransitionFinished;
    }

    private void Update()
    {
        if (!_transitioning)
            return;
        
        transform.position = _cameraController.ScreenToWorldPointy(Pointer.current.position.ReadValue());
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!CanDrag())
            return;
        
        transform.position = _cameraController.ScreenToWorldPointy(eventData.position);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanDrag())
            return;

        if (_sprite != null)
            _sprite.sortingOrder = 9;
        
        _holdOffset = transform.position - eventData.pointerCurrentRaycast.worldPosition;
        Held(eventData);
        _dragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!CanDrag())
            return;
        
        if (_sprite != null)
            _sprite.sortingOrder = _order;
        
        _holdOffset = Vector2.zero;
        Dropped(eventData);
        _dragging = false;
    }
    
    public void AddCanDragHandler(Func<bool> handler) => _canDragHandlers.Add(handler);
    
    public void RemoveCanDragHandler(Func<bool> handler) =>  _canDragHandlers.Remove(handler);

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
    
    private bool CanDrag() => _canDragHandlers.Aggregate(true, (agg, handler) => agg && handler());
}
