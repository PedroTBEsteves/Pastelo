public interface ITooltipPresenter
{
    TooltipView GetViewPrefab(TooltipTarget target);
    bool Configure(TooltipView view, TooltipTarget target);
}
