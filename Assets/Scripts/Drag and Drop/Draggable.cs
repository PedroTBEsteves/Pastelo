using System;
using System.Collections.Generic;
using System.Linq;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public sealed class Draggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Inject]
    private readonly CameraController _cameraController;

    [Inject]
    private readonly DraggableInputConfiguration _inputConfiguration;

    [SerializeField]
    private bool _forceDraggableLayer = true;
    
    private Vector2 _holdOffset;
    
    private SpriteRenderer _sprite;
    private int _order;
    
    private bool _dragging;
    private bool _followPointerContinuously;
    private bool _transitioning;
    private bool _pendingEndDrag;
    private bool _pendingPointerClickIgnore;
    private bool _ignoreNextPointerClick;
    private PointerEventData _pendingEndDragEventData;
    
    private readonly List<Func<bool>> _canDragHandlers = new();
    
    public event Action<PointerEventData> Held = delegate { };
    public event Action<PointerEventData> Dropped = delegate { };
    
    public bool IsDragging => _dragging;
    
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

    public void OnPointerDown(PointerEventData eventData) => _inputConfiguration.CurrentHandler.OnPointerDown(this, eventData);

    public void OnPointerUp(PointerEventData eventData) => _inputConfiguration.CurrentHandler.OnPointerUp(this, eventData);

    public void OnDrag(PointerEventData eventData) => _inputConfiguration.CurrentHandler.OnDrag(this, eventData);

    public void OnBeginDrag(PointerEventData eventData) => _inputConfiguration.CurrentHandler.OnBeginDrag(this, eventData);

    public void OnEndDrag(PointerEventData eventData) => _inputConfiguration.CurrentHandler.OnEndDrag(this, eventData);
    
    public void OnPointerClick(PointerEventData eventData) => _inputConfiguration.CurrentHandler.OnPointerClick(this, eventData);
    
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

    public void BeginPendingPointerClickIgnore() => _pendingPointerClickIgnore = true;

    public void FinalizePendingPointerClickIgnore(bool shouldIgnore)
    {
        _ignoreNextPointerClick = _pendingPointerClickIgnore && shouldIgnore;
        _pendingPointerClickIgnore = false;
    }

    public bool TryConsumeIgnoredPointerClick()
    {
        if (!_ignoreNextPointerClick)
            return false;

        _ignoreNextPointerClick = false;
        return true;
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
        _pendingPointerClickIgnore = false;
        _ignoreNextPointerClick = false;
        _transitioning = false;
        _dragging = false;
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
