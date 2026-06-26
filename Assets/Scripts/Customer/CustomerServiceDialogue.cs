using System.Collections.Generic;
using PrimeTween;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Localization;

public class CustomerServiceDialogue : MonoBehaviour, ICustomerServiceDialogue
{
    [SerializeField]
    private LocalizedStringTable _orderDialogueTable;

    [SerializeField]
    private LocalizedStringTable _emptyPastelDialogueTable;

    [SerializeField]
    private LocalizedStringTable _failedOrderDialoguesTable;

    [SerializeField]
    private LocalizedStringTable _ingredientsTable;

    [SerializeField]
    private LocalizedStringTable _dialogueFormattingTable;

    [Inject]
    private readonly ICustomerPopUpDialogue _customerPopUpDialogue;

    private Sequence _dialogueSequence;
    public bool IsPlaying => _dialogueSequence.isAlive;

    public Sequence OrderDialogue(Order order)
    {
        var text = GetOrderDialogueText(order);
        _dialogueSequence = _customerPopUpDialogue.ShowDialogue(order.Customer, text);

        return _dialogueSequence;
    }

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

        return CustomerDialogueLocalization.JoinLocalizedList(
            _dialogueFormattingTable,
            fillingsParts,
            nameof(CustomerServiceDialogue),
            nameof(_dialogueFormattingTable));
    }

    private string GetMissingIngredientsText(IReadOnlyList<Ingredient> ingredients)
    {
        if (ingredients == null || ingredients.Count == 0)
            return string.Empty;

        var ingredientParts = new List<string>(ingredients.Count);
        var seenIngredients = new HashSet<Ingredient>();

        foreach (var ingredient in ingredients)
        {
            if (ingredient == null || !seenIngredients.Add(ingredient))
                continue;

            ingredientParts.Add(CustomerDialogueLocalization.GetLocalizedIngredientName(
                _ingredientsTable,
                ingredient,
                nameof(CustomerServiceDialogue),
                nameof(_ingredientsTable)));
        }

        return CustomerDialogueLocalization.JoinLocalizedList(
            _dialogueFormattingTable,
            ingredientParts,
            nameof(CustomerServiceDialogue),
            nameof(_dialogueFormattingTable));
    }
}
