using System;
using System.Collections.Generic;
using System.Linq;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public sealed class Draggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    private enum InteractionGesture
    {
        None,
        Pending,
        Drag,
        Click
    }

    [Inject]
    private readonly CameraController _cameraController;

    [SerializeField]
    private bool _forceDraggableLayer = true;
    
    private Vector2 _holdOffset;
    
    private SpriteRenderer _sprite;
    private int _order;
    
    private bool _dragging;
    private bool _followPointerContinuously;
    private bool _transitioning;
    private bool _pendingEndDrag;
    private PointerEventData _pendingEndDragEventData;
    private InteractionGesture _activeGesture;
    
    private readonly List<Func<bool>> _canDragHandlers = new();
    
    public event Action<PointerEventData> Held = delegate { };
    public event Action<PointerEventData> Dropped = delegate { };
    
    public bool IsDragging => _dragging;
    public bool IsUsingClickGesture => _activeGesture == InteractionGesture.Click;
    
    private void Awake()
    {
        if (_forceDraggableLayer)
            gameObject.layer = LayerMask.NameToLayer("Draggable");
        _sprite = GetComponent<SpriteRenderer>();
        _order = _sprite != null ? _sprite.sortingOrder : 0;

        _cameraController.CameraBeganMoving += OnCameraTransitionStarted;
        _cameraController.CameraEndedMoving += OnCameraTransitionFinished;
    }

    private void Update()
    {
        if (!_dragging || (!_followPointerContinuously && !_transitioning && !_pendingEndDrag))
            return;

        var pointerPosition = Pointer.current.position.ReadValue();

        UpdateDragPosition(pointerPosition);
    }

    private void OnDestroy()
    {
        _cameraController.CameraBeganMoving -= OnCameraTransitionStarted;
        _cameraController.CameraEndedMoving -= OnCameraTransitionFinished;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_dragging)
            return;

        _activeGesture = InteractionGesture.Pending;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_dragging)
            return;

        UpdateDragPosition(eventData.position);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_dragging || _activeGesture != InteractionGesture.Pending || !CanDrag())
            return;

        BeginPointerDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_dragging || _activeGesture != InteractionGesture.Drag)
            return;

        EndDrag(eventData);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_dragging)
        {
            if (_activeGesture == InteractionGesture.Click)
                EndDrag(eventData);

            return;
        }

        if (_activeGesture != InteractionGesture.Pending)
            return;

        BeginClickDrag(eventData);
    }
    
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
        _pendingEndDrag = false;
        _pendingEndDragEventData = null;
        _dragging = true;
        Held(eventData);
    }

    public void BeginPointerDrag(PointerEventData eventData)
    {
        BeginDrag(eventData);
        if (_dragging)
            _activeGesture = InteractionGesture.Drag;
    }

    public void BeginClickDrag(PointerEventData eventData)
    {
        BeginDrag(eventData, followPointerContinuously: true);
        if (!_dragging)
            return;

        _activeGesture = InteractionGesture.Click;
        UpdateDragPosition(eventData.position);
    }

    public void BeginPendingPointerClickIgnore() { }

    public void FinalizePendingPointerClickIgnore(bool shouldIgnore) { }

    public bool TryConsumeIgnoredPointerClick() => false;

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

        if (_transitioning || _cameraController.IsMoving)
        {
            _pendingEndDrag = true;
            _pendingEndDragEventData = eventData;
            _followPointerContinuously = false;
            return;
        }

        CompleteDrag(eventData);
    }

    private void CompleteDrag(PointerEventData eventData)
    {
        if (_sprite != null)
            _sprite.sortingOrder = _order;

        _holdOffset = Vector2.zero;
        _followPointerContinuously = false;
        _pendingEndDrag = false;
        _pendingEndDragEventData = null;
        _transitioning = false;
        _dragging = false;
        _activeGesture = InteractionGesture.None;
        Dropped(eventData);
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

        if (_pendingEndDrag)
            CompleteDrag(_pendingEndDragEventData);
    }
}
