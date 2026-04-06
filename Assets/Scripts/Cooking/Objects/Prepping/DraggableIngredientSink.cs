using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(DraggableSink))]
public abstract class DraggableIngredientSink<TIngredient> : ValidatedMonoBehaviour, IPointerDownHandler where TIngredient : Ingredient
{
    [SerializeField] 
    private TIngredient _ingredient;
    
    [SerializeField, Child(Flag.ExcludeSelf)]
    private Transform _lockedIndicator;

    [SerializeField, Self]
    private SpriteRenderer _spriteRenderer;
    
    [SerializeField, Self]
    private DraggableSink _draggableSink;

    [SerializeField, Scene]
    private IngredientUnlockPurchasePrompt _purchasePrompt;
    
    [Inject]
    private readonly IngredientsStorage _ingredientsStorage;

    [Inject]
    private readonly GameplayInteractionGate _interactionGate;

    [Inject]
    private readonly TutorialTargetRegistry _tutorialTargetRegistry;

    private TutorialTarget _tutorialTarget;

    private void Awake()
    {
        var isUnlocked = _ingredientsStorage.Contains(_ingredient);
        
        _lockedIndicator.gameObject.SetActive(!isUnlocked);
        _spriteRenderer.enabled = isUnlocked;

        _tutorialTarget = GetComponent<TutorialTarget>() ?? gameObject.AddComponent<TutorialTarget>();
        _tutorialTarget.Configure(typeof(TIngredient) == typeof(Dough) ? TutorialTargetId.DoughSource : TutorialTargetId.FillingSource, _ingredient);
        _tutorialTargetRegistry.Register(_tutorialTarget);
        
        _draggableSink.AddCanCreateDraggableHandler(CanCreateDraggable);
    }

    private void OnDestroy()
    {
        _draggableSink.RemoveCanCreateDraggableHandler(CanCreateDraggable);
        _tutorialTargetRegistry.Unregister(_tutorialTarget);
    }

    private bool CanCreateDraggable()
    {
        var interactionType = typeof(TIngredient) == typeof(Dough)
            ? TutorialInteractionType.UseDough
            : TutorialInteractionType.AddFilling;

        return _ingredientsStorage.Contains(_ingredient)
               && _interactionGate.CanInteract(interactionType, _ingredient);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        var isUnlocked = _ingredientsStorage.Contains(_ingredient);

        if (isUnlocked)
            return;

        if (!_interactionGate.CanInteract(TutorialInteractionType.BuyIngredient, _ingredient))
            return;

        _purchasePrompt.Show(_ingredient, OnIngredientPurchased);
    }

    private void OnIngredientPurchased()
    {
        _lockedIndicator.gameObject.SetActive(false);
        _spriteRenderer.enabled = true;
    }
}
