using System;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Draggable))]
public class DraggableClosedPastel : ValidatedMonoBehaviour
{
    private const float TutorialBurnProtectionProgress = 0.90f;

    [SerializeField, Self]
    private SpriteRenderer _spriteRenderer;

    [SerializeField, Self]
    private BoxCollider2D _collider;

    [SerializeField, Child(Flag.Editable)]
    private Slider _rawSlider;

    [SerializeField, Child(Flag.Editable)]
    private Slider _cookedSlider;
    
    [SerializeField, Self]
    private Draggable _draggable;

    [Inject]
    private readonly CameraController _cameraController;
    
    [Inject]
    private readonly TimeController _timeController;

    [Inject]
    private readonly GameplayTutorialEvents _tutorialEvents;

    [Inject]
    private readonly GameplayInteractionGate _interactionGate;

    [Inject]
    private readonly TutorialTargetRegistry _tutorialTargetRegistry;

    [Inject]
    private readonly GameplayTutorialState _tutorialState;
    
    private ClosedPastelDough _closedPastelDough;

    private FryingArea _fryingArea;
    private Vector3 _dragStartPosition;
    private bool _frying;

    private Slider _activeSlider;
    private TutorialTarget _tutorialTarget;
    private TooltipTarget _tooltipTarget;

    private void Awake()
    {
        _tutorialTarget = GetComponent<TutorialTarget>() ?? gameObject.AddComponent<TutorialTarget>();
        _tutorialTarget.Configure(TutorialTargetId.CookedPastel, this);
        _tutorialTargetRegistry.Register(_tutorialTarget);

        _tooltipTarget = GetComponent<TooltipTarget>();
        var tooltipPresenter = GetComponent<ClosedPastelTooltipPresenter>() ?? gameObject.AddComponent<ClosedPastelTooltipPresenter>();
        _tooltipTarget?.Configure(presenter: tooltipPresenter);

        _draggable.Held += OnHeld;
        _draggable.Dropped += OnDropped;
        _draggable.AddCanDragHandler(CanDragPastel);
    }

    private void OnDestroy()
    {
        _tutorialTargetRegistry.Unregister(_tutorialTarget);
        _draggable.Held -= OnHeld;
        _draggable.Dropped -= OnDropped;
        _draggable.RemoveCanDragHandler(CanDragPastel);
        if (_closedPastelDough != null)
            _closedPastelDough.FriedLevelChanged -= OnFriedLevelChanged;
    }

    private void Start()
    {
        _rawSlider.gameObject.SetActive(false);
        _cookedSlider.gameObject.SetActive(false);
    }

    public void Initialize(ClosedPastelDough closedPastelDough)
    {
        _closedPastelDough = closedPastelDough;
        _closedPastelDough.FriedLevelChanged += OnFriedLevelChanged;
        OnFriedLevelChanged(_closedPastelDough.FriedLevel);
        if (closedPastelDough.FriedLevel != FriedLevel.Raw)
            _rawSlider.normalizedValue = 1f;
    }

    public ClosedPastelDough GetClosedPastelDough() => _closedPastelDough;
    public Pastel GetPastel() => _closedPastelDough.Finish();

    private void OnFriedLevelChanged(FriedLevel level)
    {
        _spriteRenderer.sprite = _closedPastelDough.Dough.GetClosedDoughSprite(level);

        _activeSlider = level switch
        {
            FriedLevel.Raw => _rawSlider,
            FriedLevel.Done or FriedLevel.Burnt => _cookedSlider,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };

        _activeSlider.normalizedValue = _closedPastelDough.FryingProgress;

        if (level == FriedLevel.Done)
            _tutorialEvents.PublishPastelReachedCooked(this);
    }
        

    public void Fry(float deltaTime)
    {
        if (ShouldLimitTutorialFrying())
        {
            deltaTime = Mathf.Min(
                deltaTime,
                _closedPastelDough.GetRemainingTimeUntilProgress(TutorialBurnProtectionProgress));

            if (deltaTime <= 0f)
            {
                _activeSlider.normalizedValue = _closedPastelDough.FryingProgress;
                return;
            }
        }

        _closedPastelDough.Fry(deltaTime);

        _activeSlider.normalizedValue = _closedPastelDough.FryingProgress;
    }
    
    private void SetFrying(bool frying)
    {
        _frying = frying;
        _rawSlider.gameObject.SetActive(_frying);
        _cookedSlider.gameObject.SetActive(_frying);
        _spriteRenderer.sortingOrder = frying ? 2 : 4;
    }
    
    private void OnHeld(PointerEventData eventData)
    {
        _tutorialEvents.PublishPastelPickedUp(this);

        if (_frying && _interactionGate.CanInteract(TutorialInteractionType.RemoveCookedPastel, this))
            _tutorialEvents.PublishPastelRemovedFromFryer(this);

        SetFrying(false);
        _fryingArea?.Remove(this);
        _dragStartPosition = _cameraController.ScreenToWorldPointy(eventData.position);
    }

    private void OnDropped(PointerEventData eventData)
    {
        _tutorialEvents.PublishPastelDropped(this);

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

    private bool CanDragPastel()
    {
        if (!_frying)
            return true;

        return _interactionGate.CanInteract(TutorialInteractionType.RemoveCookedPastel, this);
    }

    private bool ShouldLimitTutorialFrying()
    {
        return _tutorialState.IsActive
            && _tutorialState.TutorialPastel == this
            && _closedPastelDough.FriedLevel == FriedLevel.Done;
    }
}
