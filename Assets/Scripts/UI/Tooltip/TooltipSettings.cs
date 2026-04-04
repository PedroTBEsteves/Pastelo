using UnityEngine;

[CreateAssetMenu(fileName = "TooltipSettings", menuName = "Scriptable Objects/UI/TooltipSettings")]
public class TooltipSettings : ScriptableObject
{
    [field: SerializeField, Min(0f)]
    public float HoverDelay { get; private set; } = 0.5f;

    [field: SerializeField]
    public Vector2 ScreenOffset { get; private set; } = new(16f, -16f);

    public static TooltipSettings CreateFallback()
    {
        var settings = CreateInstance<TooltipSettings>();
        settings.HoverDelay = 0.5f;
        settings.ScreenOffset = new Vector2(16f, -16f);
        return settings;
    }
}
