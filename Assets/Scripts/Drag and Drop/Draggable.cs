using System;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Draggable : ValidatedMonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField]
    private bool _isDisposable;
    
    [Inject]
    private readonly CameraController _cameraController;

    [Inject]
    private readonly TrashBin _trashBin;
    
    [Inject]
    private readonly SectionController _sectionController;
    
    private Vector2 _holdOffset;
    
    protected virtual void OnHold(PointerEventData eventData) { }
    protected virtual void OnDrop(PointerEventData eventData) { }

    protected virtual bool CanDrag() => true;
    
    private SpriteRenderer _sprite;
    private int _order;
    
    private bool _dragging;
    private bool _transitioning;
    
    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Draggable");
        _sprite = GetComponent<SpriteRenderer>();
        _order = _sprite != null ? _sprite.sortingOrder : 0;

        _cameraController.CameraBeganMoving += OnCameraTransitionStarted;
        _cameraController.CameraEndedMoving += OnCameraTransitionFinished;
    }

    protected virtual void Update()
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
        transform.parent = null;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanDrag())
            return;

        if (_sprite != null)
            _sprite.sortingOrder = 9;
        
        _holdOffset = transform.position - eventData.pointerCurrentRaycast.worldPosition;
        OnHold(eventData);
        _dragging = true;
        _trashBin.Show();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!CanDrag())
            return;
        
        if (_sprite != null)
            _sprite.sortingOrder = _order;
        
        if (_isDisposable)
        {
            if (_trashBin.IsInside(eventData))
            {
                _trashBin.PlayDiscardSound();
                _trashBin.Hide();
                Destroy(gameObject);
                return;
            }
            
            _trashBin.Hide();
        }
        
        _holdOffset = Vector2.zero;
        _sectionController.AttachToSection(transform);
        OnDrop(eventData);
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
