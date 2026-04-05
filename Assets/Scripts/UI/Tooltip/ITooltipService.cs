using UnityEngine;

public interface ITooltipService
{
    bool Show(TooltipTarget target, Vector2 screenPosition, Camera eventCamera);
    void UpdatePosition(TooltipTarget target, Vector2 screenPosition, Camera eventCamera);
    void Hide(TooltipTarget target);
}
