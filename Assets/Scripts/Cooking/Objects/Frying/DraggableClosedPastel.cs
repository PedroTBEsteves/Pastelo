using System;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableClosedPastel : Draggable
{
    [SerializeField, Self]
    private SpriteRenderer _spriteRenderer;

    [SerializeField, Self]
    private BoxCollider2D _collider;

    [SerializeField, Child(Flag.Editable)]
    private Slider _rawSlider;

    [SerializeField, Child(Flag.Editable)]
    private Slider _cookedSlider;

    [Inject]
    private readonly CameraController _cameraController;
    
    [Inject]
    private readonly TimeController _timeController;
    
    private ClosedPastelDough _closedPastelDough;

    private FryingArea _fryingArea;
    private Vector3 _dragStartPosition;
    private bool _frying;

    private Slider _activeSlider;

    private void Start()
    {
        _rawSlider.gameObject.SetActive(false);
        _cookedSlider.gameObject.SetActive(false);
    }

    public void Initialize(ClosedPastelDough closedPastelDough)
    {
        _closedPastelDough = closedPastelDough;
        _closedPastelDough.FriedLevelChanged += OnFriedLevelChanged;
        OnFriedLevelChanged(FriedLevel.Raw);
    }

    public Pastel GetPastel() => _closedPastelDough.Finish();

    private void OnFriedLevelChanged(FriedLevel level)
    {
        _spriteRenderer.sprite = _closedPastelDough.Dough.GetClosedDoughSprite(level);

        switch (level)
        {
            case FriedLevel.Raw:
                _activeSlider = _rawSlider;
                break;
            case FriedLevel.Done:
                _activeSlider = _cookedSlider;
                break;
            case FriedLevel.Burnt:
                _activeSlider = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(level), level, null);
        }
    }
        

    protected override void Update()
    {
        base.Update();
        
        if (!_frying || !_timeController.Running)
            return;
        
        _closedPastelDough.Fry(Time.deltaTime);

        if (_activeSlider != null)
            _activeSlider.normalizedValue = _closedPastelDough.FryingProgress;
    }
    
    private void SetFrying(bool frying)
    {
        _frying = frying;
        _rawSlider.gameObject.SetActive(_frying);
        _cookedSlider.gameObject.SetActive(_frying);
    }
    
    protected override void OnHold(PointerEventData eventData)
    {
        SetFrying(false);
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
        {
            SetFrying(true);
            return;
        }
            

        if (CheckDelivery(raycastHit))
        {
            Destroy(gameObject);
            return;
        }
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
}
