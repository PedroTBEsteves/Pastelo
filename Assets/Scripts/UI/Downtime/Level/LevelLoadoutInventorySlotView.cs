using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelLoadoutInventorySlotView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
    private Image _iconImage;

    [SerializeField]
    private TMP_Text _nameText;

    [SerializeField]
    private TMP_Text _quantityText;

    [SerializeField]
    private GameObject _quantityRoot;

    [SerializeField]
    private Color _defaultIconColor = Color.white;

    [SerializeField]
    private Color _depletedIconColor = Color.gray;

    private LevelLoadoutEditorView _editor;
    private LoadoutInventoryProjectionEntry _entry;
    private bool _hasPendingPreview;

    public void Bind(LevelLoadoutEditorView editor, LoadoutInventoryProjectionEntry entry)
    {
        _editor = editor;
        _entry = entry;
        _hasPendingPreview = false;
        Refresh();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_editor == null || _entry.Ingredient == null || _entry.AvailableQuantity <= 0)
            return;

        _editor.TryBeginInventoryPreviewDrag(this, _entry.Ingredient, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        _editor?.HandleDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _editor?.EndDrag(eventData);
    }

    public void BeginPendingPreview()
    {
        if (_hasPendingPreview)
            return;

        _hasPendingPreview = true;
        Refresh();
    }

    public void CancelPendingPreview()
    {
        if (!_hasPendingPreview)
            return;

        _hasPendingPreview = false;
        Refresh();
    }

    public void ConfirmPendingPreview()
    {
        if (!_hasPendingPreview)
            return;

        _hasPendingPreview = false;
        Refresh();
    }

    private void Refresh()
    {
        var displayedQuantity = GetDisplayedQuantity();

        if (_iconImage != null)
        {
            _iconImage.sprite = _entry.Ingredient != null ? _entry.Ingredient.Icon : null;
            _iconImage.color = _entry.Ingredient != null && displayedQuantity <= 0
                ? _depletedIconColor
                : _defaultIconColor;
        }

        if (_nameText != null)
            _nameText.SetText(_entry.Ingredient != null ? _entry.Ingredient.GetDisplayName() : string.Empty);

        if (_quantityRoot != null)
            _quantityRoot.SetActive(_entry.Ingredient != null);

        if (_quantityText != null)
            _quantityText.SetText(_entry.Ingredient != null ? displayedQuantity.ToString() : string.Empty);
    }

    private int GetDisplayedQuantity()
    {
        if (_entry.Ingredient == null)
            return 0;

        return Mathf.Max(0, _entry.AvailableQuantity - (_hasPendingPreview ? 1 : 0));
    }
}
