using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(DraggableSource))]
public abstract class DraggableIngredientSource<TIngredient> : ValidatedMonoBehaviour, IPointerDownHandler where TIngredient : Ingredient
{
    [SerializeField] 
    private TIngredient _ingredient;
    
    [SerializeField, Child(Flag.ExcludeSelf)]
    private Transform _lockedIndicator;

    [SerializeField, Self]
    private SpriteRenderer _spriteRenderer;
    
    [SerializeField, Self]
    private DraggableSource _draggableSource;

    [SerializeField, Scene]
    private IngredientUnlockPurchasePrompt _purchasePrompt;

    [Inject]
    private readonly GameplayInteractionGate _interactionGate;

    [Inject]
    private readonly TutorialTargetRegistry _tutorialTargetRegistry;

    private TutorialTarget _tutorialTarget;
    private bool _isInitialized;

    public TIngredient Ingredient => _ingredient;

    private void Awake()
    {
        _tutorialTarget = GetComponent<TutorialTarget>() ?? gameObject.AddComponent<TutorialTarget>();
        _tutorialTargetRegistry.Register(_tutorialTarget);

        _draggableSource.AddCanCreateDraggableHandler(CanCreateDraggable);
        _draggableSource.AddDraggableCreatedHandler(OnDraggableCreated);

        _isInitialized = true;
        RefreshConfiguredState();
    }

    private void OnDestroy()
    {
        _draggableSource.RemoveCanCreateDraggableHandler(CanCreateDraggable);
        _draggableSource.RemoveDraggableCreatedHandler(OnDraggableCreated);
        _tutorialTargetRegistry.Unregister(_tutorialTarget);
    }

    public void Configure(TIngredient ingredient)
    {
        if (ingredient == null)
            throw new System.ArgumentNullException(nameof(ingredient));

        _ingredient = ingredient;
        RefreshConfiguredState();
    }

    private bool CanCreateDraggable()
    {
        if (_ingredient == null)
            return false;

        var interactionType = typeof(TIngredient) == typeof(Dough)
            ? TutorialInteractionType.UseDough
            : TutorialInteractionType.AddFilling;

        return _interactionGate.CanInteract(interactionType, _ingredient);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_ingredient == null)
            return;
    }

    private void RefreshConfiguredState()
    {
        var hasIngredient = _ingredient != null;

        if (_lockedIndicator != null)
            _lockedIndicator.gameObject.SetActive(false);

        if (_spriteRenderer != null)
        {
            _spriteRenderer.enabled = hasIngredient;

            if (hasIngredient)
                _spriteRenderer.sprite = _ingredient.SourceSprite;
        }

        if (!_isInitialized || _tutorialTarget == null || !hasIngredient)
            return;

        var tutorialTargetId = typeof(TIngredient) == typeof(Dough)
            ? TutorialTargetId.DoughSource
            : TutorialTargetId.FillingSource;

        _tutorialTarget.Configure(tutorialTargetId, _ingredient);
    }

    private void OnDraggableCreated(Draggable draggable)
    {
        if (_ingredient == null || draggable == null)
            return;

        if (!draggable.TryGetComponent(out DraggableIngredient<TIngredient> draggableIngredient))
        {
            Debug.LogError($"{nameof(DraggableSource)} created a draggable without a matching {nameof(DraggableIngredient<TIngredient>)} on '{draggable.name}'.", draggable);
            return;
        }

        draggableIngredient.Configure(_ingredient);
    }
}
