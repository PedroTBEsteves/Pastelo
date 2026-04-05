using UnityEngine;

public class ClosedPastelTooltipPresenter : MonoBehaviour, ITooltipPresenter
{
    private const string TooltipViewResourcePath = "UI/Tooltip/OrderNoteTooltipView";

    private DraggableClosedPastel _closedPastel;
    private OrderNoteTooltipView _viewPrefab;

    public TooltipView GetViewPrefab(TooltipTarget target)
    {
        if (_viewPrefab == null)
            _viewPrefab = Resources.Load<OrderNoteTooltipView>(TooltipViewResourcePath);

        return _viewPrefab;
    }

    public bool Configure(TooltipView view, TooltipTarget target)
    {
        if (_closedPastel == null)
            _closedPastel = GetComponent<DraggableClosedPastel>();

        var recipe = _closedPastel?.GetClosedPastelDough()?.Recipe;

        if (recipe == null || view is not OrderNoteTooltipView orderNoteTooltipView)
            return false;

        return orderNoteTooltipView.Bind(recipe);
    }
}
