using System;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableClosedPastel : Draggable
{
    [SerializeField, Self]
    private SpriteRenderer _spriteRenderer;

    [SerializeField, Self]
    private BoxCollider2D _collider;

    [Inject]
    private readonly CameraController _cameraController;
    
    [Inject]
    private readonly TimeController _timeController;
    
    private ClosedPastelDough _closedPastelDough;

    private FryingArea _fryingArea;
    private Vector3 _dragStartPosition;
    private bool _raised = true;
    
    public void Initialize(ClosedPastelDough closedPastelDough)
    {
        _closedPastelDough = closedPastelDough;
        _closedPastelDough.FriedLevelChanged += UpdateSprite;
        UpdateSprite(FriedLevel.Raw);
    }
    
    public void ToggleRaised()
    {
        _raised = !_raised;
        _collider.enabled = _raised;
    }

    public Pastel GetPastel() => _closedPastelDough.Finish();

    private void UpdateSprite(FriedLevel level) =>
        _spriteRenderer.sprite = _closedPastelDough.Dough.GetClosedDoughSprite(level);

    protected override void Update()
    {
        base.Update();
        
        if (_raised || !_timeController.Running)
            return;
        
        _closedPastelDough.Fry(Time.deltaTime);
    }
    
    protected override void OnHold(PointerEventData eventData)
    {
        _fryingArea?.Remove(this);
        _dragStartPosition = _cameraController.ScreenToWorldPointy(eventData.position);
    }

    protected override void OnDrop(PointerEventData eventData)
    {
        var mousePosition = _cameraController.ScreenToWorldPointy(eventData.position);
        var raycastHit = Physics2D.Raycast(
            mousePosition,
            Vector2.zero,
            float.MaxValue,
            ~LayerMask.GetMask("Draggable"));
        
        if (!raycastHit)
            return;

        if (CheckFryingArea(raycastHit, mousePosition))
            return;

        if (CheckDelivery(raycastHit))
        {
            Destroy(gameObject);
            return;
        }
        
        transform.position = _dragStartPosition;
    }

    private bool CheckFryingArea(RaycastHit2D raycastHit2D, Vector2 mousePosition)
    {
        if (!raycastHit2D.collider.TryGetComponent(out _fryingArea))
            return false;

        if (!_fryingArea.TryAdd(this, mousePosition))
            transform.position = _fryingArea.DiscardPosition;
        
        return true;
    }

    private bool CheckDelivery(RaycastHit2D raycastHit2D)
    {
        if (!raycastHit2D.collider.TryGetComponent<Deliverable>(out var deliverable))
            return false;
        
        if (!deliverable.TryAddPastel(this))
            transform.position = _fryingArea.DiscardPosition;
        
        return true;
    }

    protected override bool CanDrag() => _raised;
}
