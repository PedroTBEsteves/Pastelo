using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderNoteTooltipView : TooltipView
{
    [SerializeField]
    private Image _doughIcon;

    [SerializeField]
    private GameObject _emptyIngredients;

    [SerializeField]
    private Transform _ingredientsRoot;

    [SerializeField]
    private OrderNoteTooltipIngredientRow _ingredientRowPrefab;

    public bool Bind(Order order)
    {
        return order != null && Bind(order.Recipe);
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

        ClearIngredients();
        _doughIcon.sprite = recipe.Dough.Icon;
        _doughIcon.preserveAspect = true;

        var hasIngredients = false;

        foreach (var (filling, amount) in recipe.Fillings)
        {
            var ingredientRow = Instantiate(_ingredientRowPrefab, _ingredientsRoot);
            ingredientRow.Bind(filling, amount);
            hasIngredients = true;
        }

        if (_emptyIngredients != null)
            _emptyIngredients.SetActive(!hasIngredients);

        return true;
    }

    public override bool Bind(TooltipTarget target)
    {
        if (target == null || !target.TryGetComponent<OrderNote>(out var orderNote))
            return false;

        return Bind(orderNote.Order);
    }

    private void ClearIngredients()
    {
        for (var i = _ingredientsRoot.childCount - 1; i >= 0; i--)
            Destroy(_ingredientsRoot.GetChild(i).gameObject);
    }
}
