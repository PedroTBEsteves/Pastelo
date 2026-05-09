using KBCore.Refs;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelSelectionButton : ValidatedMonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField, Scene]
    private LevelLoadoutEditorView _levelLoadoutEditorView;

    [SerializeField, Scene]
    private Canvas _canvas;

    [SerializeField, Self]
    private Button _button;

    [SerializeField, Self]
    private Image _levelImage;

    [SerializeField]
    private GameObject _levelInfoPanel;
    
    [SerializeField]
    private TextMeshProUGUI _levelNameText;
    
    [SerializeField]
    private TextMeshProUGUI _levelPriceText;

    [SerializeField]
    private Level _level;

    private int _siblingIndex;

    private void Awake()
    {
        _button.onClick.AddListener(OnButtonClicked);
        _levelNameText.SetText(_level.Name.GetLocalizedString());
        _levelPriceText.SetText(TextUtils.FormatAsMoney(_level.PriceToPlay));
        _levelInfoPanel.SetActive(false);
        _levelImage.alphaHitTestMinimumThreshold = 0.9f;
        _siblingIndex = transform.GetSiblingIndex();
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        _levelLoadoutEditorView.Show(_level);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _levelInfoPanel.SetActive(true);
        transform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _levelInfoPanel.SetActive(false);
        transform.SetSiblingIndex(_siblingIndex);
    }
}
