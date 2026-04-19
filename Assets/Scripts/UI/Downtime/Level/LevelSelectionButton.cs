using KBCore.Refs;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionButton : ValidatedMonoBehaviour
{
    [SerializeField, Scene]
    private LevelLoadoutEditorView _levelLoadoutEditorView;

    [SerializeField, Self]
    private Button _button;
    
    [SerializeField, Child]
    private TextMeshProUGUI _levelNameText;

    [SerializeField]
    private Level _level;

    private void Awake()
    {
        _button.onClick.AddListener(OnButtonClicked);
        _levelNameText.SetText($"{_level.Name.GetLocalizedString()} ({TextUtils.FormatAsMoney(_level.PriceToPlay)})");
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        _levelLoadoutEditorView.Show(_level);
    }
}
