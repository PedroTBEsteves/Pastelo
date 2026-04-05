using TMPro;
using UnityEngine;

public class OrderNoteTooltipView : TooltipView
{
    [SerializeField]
    private TextMeshProUGUI _doughName;

    [SerializeField]
    private GameObject _emptyIngredients;

    [SerializeField]
    private Transform _ingredientsRoot;

    [SerializeField]
    private OrderNoteTooltipIngredientRow _ingredientRowPrefab;

    public bool Bind(Order order)
    {
        if (order == null
            || order.Recipe == null
            || _doughName == null
            || _ingredientsRoot == null
            || _ingredientRowPrefab == null)
        {
            return false;
        }

        ClearIngredients();
        _doughName.SetText(order.Recipe.Dough.Name);

        var hasIngredients = false;

        foreach (var (filling, amount) in order.Recipe.Fillings)
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
