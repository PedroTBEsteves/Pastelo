using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryIngredientHint : MonoBehaviour
{
    [SerializeField]
    private Image _doughIcon;

    [SerializeField]
    private Transform _ingredientsRoot;

    [SerializeField]
    private OrderNoteTooltipIngredientRow _ingredientRowPrefab;

    private readonly List<OrderNoteTooltipIngredientRow> _ingredientRows = new();

    private void Awake()
    {
        if (transform is not RectTransform)
            Debug.LogError($"{nameof(DeliveryIngredientHint)} on '{name}' must be a UI object with a RectTransform.", this);
    }

    public void Bind(Recipe recipe)
    {
        ClearIngredientRows();

        if (recipe == null)
        {
            if (_doughIcon != null)
                _doughIcon.enabled = false;

            return;
        }

        if (_doughIcon != null)
        {
            _doughIcon.sprite = recipe.Dough.Icon;
            _doughIcon.preserveAspect = true;
            _doughIcon.enabled = recipe.Dough.Icon != null;
        }

        foreach (var (filling, amount) in recipe.Fillings)
            AddIngredientRow(filling, amount);
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    private void AddIngredientRow(Filling filling, int amount)
    {
        if (_ingredientsRoot == null || _ingredientRowPrefab == null)
        {
            Debug.LogError($"{nameof(DeliveryIngredientHint)} on '{name}' is missing UI ingredient row references.", this);
            return;
        }

        var row = Instantiate(_ingredientRowPrefab, _ingredientsRoot);
        row.Bind(filling, amount);
        _ingredientRows.Add(row);
    }

    private void ClearIngredientRows()
    {
        foreach (var row in _ingredientRows)
        {
            if (row != null)
                Destroy(row.gameObject);
        }

        _ingredientRows.Clear();
    }
}
