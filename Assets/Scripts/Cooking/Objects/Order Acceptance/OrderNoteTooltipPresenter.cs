using UnityEngine;

public class OrderNoteTooltipPresenter : MonoBehaviour, ITooltipPresenter
{
    [SerializeField]
    private OrderNoteTooltipView _viewPrefab;

    [SerializeField]
    private OrderNote _orderNote;

    public TooltipView GetViewPrefab(TooltipTarget target)
    {
        return _viewPrefab;
    }

    public bool Configure(TooltipView view, TooltipTarget target)
    {
        if (_orderNote == null)
            _orderNote = GetComponent<OrderNote>();

        if (_orderNote == null || _orderNote.Order == null || view is not OrderNoteTooltipView orderNoteTooltipView)
            return false;

        return orderNoteTooltipView.Bind(_orderNote.Order);
    }
}
