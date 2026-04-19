using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelPreferenceView : ValidatedMonoBehaviour
{
    [SerializeField, Child(Flag.Editable | Flag.ExcludeSelf)]
    private Image _iconImage;

    [SerializeField, Child(Flag.Editable | Flag.ExcludeSelf)]
    private TextMeshProUGUI _tagNameText;

    public void Bind(Ingredient ingredient)
    {
        if (_iconImage != null)
        {
            _iconImage.sprite = ingredient.Icon;
            _iconImage.enabled = true;
        }

        if (_tagNameText != null)
        {
            _tagNameText.SetText(string.Empty);
            _tagNameText.gameObject.SetActive(false);
        }
    }

    public void Bind(FillingTag fillingTag)
    {
        if (_iconImage != null)
        {
            _iconImage.sprite = null;
            _iconImage.enabled = false;
        }

        if (_tagNameText != null)
        {
            _tagNameText.SetText(fillingTag.Name.GetLocalizedString());
            _tagNameText.gameObject.SetActive(true);
        }
    }
}
