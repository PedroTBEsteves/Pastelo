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

    public virtual void SetText(string text)
    {
        if (_text == null)
            return;

        _text.SetText(text);
    }
}
