using System.Collections.Generic;
using PrimeTween;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Localization;

public class CustomerServiceDialogue : MonoBehaviour, ICustomerServiceDialogue
{
    [SerializeField]
    private CustomerAnimationController _customerAnimation;

    [SerializeField]
    private Transform _dialoguePosition;

    [SerializeField]
    private LocalizedStringTable _orderDialogueTable;

    [SerializeField]
    private LocalizedStringTable _emptyPastelDialogueTable;

    [SerializeField]
    private LocalizedStringTable _failedOrderDialoguesTable;

    [SerializeField]
    private LocalizedStringTable _ingredientsTable;

    [SerializeField]
    private float _delayAfterTextIsDone = 1f;

    [Inject]
    private readonly TutorialTargetRegistry _tutorialTargetRegistry;

    [Inject]
    private readonly DialoguePresentationService _dialoguePresentation;

    private Sequence _dialogueSequence;
    private TutorialTarget _tutorialTarget;

    public bool IsPlaying => _dialogueSequence.isAlive || (_customerAnimation != null && _customerAnimation.IsDialoguePlaying);

    private void Awake()
    {
        _tutorialTarget = GetComponent<TutorialTarget>() ?? gameObject.AddComponent<TutorialTarget>();
        _tutorialTarget.Configure(TutorialTargetId.OrderBell);
        _tutorialTargetRegistry.Register(_tutorialTarget);
    }

    private void OnDestroy()
    {
        if (_tutorialTarget != null)
            _tutorialTargetRegistry.Unregister(_tutorialTarget);
    }

    public Sequence OrderDialogue(Order order)
    {
        _customerAnimation.ShowDialogueCustomer(order.Customer.Sprite);

        var text = GetOrderDialogueText(order);
        _dialogueSequence = _dialoguePresentation
            .Show(text, GetDialogueWorldPosition())
            .Chain(Tween.Delay(_delayAfterTextIsDone, _customerAnimation.ShowNextCustomerAfterDialogue));

        return _dialogueSequence;
    }

    private Vector3 GetDialogueWorldPosition() => _dialoguePosition == null ? transform.position : _dialoguePosition.position;

    private string GetOrderDialogueText(Order order)
    {
        if (order.HadMissingIngredients)
        {
            var missingIngredientsText = GetMissingIngredientsText(order.MissingIngredients);
            var templateMissingEntry = CustomerDialogueLocalization.GetRandomLocalizedEntry(
                _failedOrderDialoguesTable,
                nameof(CustomerServiceDialogue),
                nameof(_failedOrderDialoguesTable));
            templateMissingEntry.IsSmart = true;
            return templateMissingEntry.GetLocalizedString(new { ingredients = missingIngredientsText });
        }

        if (order.Recipe == null)
            throw new System.InvalidOperationException("Expected a recipe for a valid order dialogue.");

        if (order.Recipe.Fillings.Count == 0)
        {
            return CustomerDialogueLocalization.GetRandomLocalizedDialogue(
                _emptyPastelDialogueTable,
                nameof(CustomerServiceDialogue),
                nameof(_emptyPastelDialogueTable));
        }

        var doughName = CustomerDialogueLocalization.GetLocalizedIngredientName(
            _ingredientsTable,
            order.Recipe.Dough,
            nameof(CustomerServiceDialogue),
            nameof(_ingredientsTable));
        var fillingsText = GetFillingsText(order.Recipe.Fillings);

        var templateEntry = CustomerDialogueLocalization.GetRandomLocalizedEntry(
            _orderDialogueTable,
            nameof(CustomerServiceDialogue),
            nameof(_orderDialogueTable));
        templateEntry.IsSmart = true;
        return templateEntry.GetLocalizedString(new { dough = doughName, fillings = fillingsText });
    }

    private string GetFillingsText(IReadOnlyDictionary<Filling, int> fillings)
    {
        var fillingsParts = new List<string>(fillings.Count);

        foreach (var (filling, amount) in fillings)
        {
            var ingredientName = CustomerDialogueLocalization.GetLocalizedIngredientName(
                _ingredientsTable,
                filling,
                nameof(CustomerServiceDialogue),
                nameof(_ingredientsTable),
                amount);
            fillingsParts.Add($"{amount} {ingredientName}");
        }

        return CustomerDialogueLocalization.JoinLocalizedList(fillingsParts);
    }

    private static string GetMissingIngredientsText(IReadOnlyList<Ingredient> ingredients)
    {
        if (ingredients == null || ingredients.Count == 0)
            return string.Empty;

        var ingredientParts = new List<string>(ingredients.Count);
        var seenIngredients = new HashSet<Ingredient>();

        foreach (var ingredient in ingredients)
        {
            if (ingredient == null || !seenIngredients.Add(ingredient))
                continue;

            ingredientParts.Add(ingredient.GetDisplayName());
        }

        return CustomerDialogueLocalization.JoinLocalizedList(ingredientParts);
    }
}
