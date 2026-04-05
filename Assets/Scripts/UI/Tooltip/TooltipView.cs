using TMPro;
using UnityEngine;

public class TooltipView : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _text;

    public RectTransform RectTransform => (RectTransform)transform;

    public void Initialize(TextMeshProUGUI text)
    {
        _text = text;
    }

    public virtual bool Bind(TooltipTarget target)
    {
        if (target == null)
            return false;

        return BindDefaultText(target.Text);
    }

    public virtual bool BindDefaultText(string text)
    {
        if (_text == null || string.IsNullOrWhiteSpace(text))
            return false;

        _text.SetText(text);
        return true;
    }
}
