using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderNoteTooltipView : TooltipView
{
    [SerializeField]
    private Image _doughIcon;

    [SerializeField]
    private Transform _ingredientsRoot;

    [SerializeField]
    private OrderNoteTooltipIngredientRow _ingredientRowPrefab;

    public bool Bind(Order order)
    {
        return order?.Recipe != null && Bind(order.Recipe);
    }

    public bool Bind(Recipe recipe)
    {
        if (recipe == null
            || _doughIcon == null
            || _ingredientsRoot == null
            || _ingredientRowPrefab == null)
        {
            return false;
        }

        _doughIcon.sprite = recipe.Dough.OrderIcon;
        _doughIcon.preserveAspect = true;
        
        foreach (var (filling, amount) in recipe.Fillings)
        {
            var ingredientRow = Instantiate(_ingredientRowPrefab, _ingredientsRoot);
            ingredientRow.Bind(filling, amount);
        }

        return true;
    }

    public override bool Bind(TooltipTarget target)
    {
        if (target == null || !target.TryGetComponent<OrderNote>(out var orderNote))
            return false;

        return Bind(orderNote.Order);
    }
}
