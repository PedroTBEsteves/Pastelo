using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;

public class TutorialTarget : MonoBehaviour
{
    [SerializeField]
    private GameObject _marker;

    [SerializeField]
    private float _highlightScaleMultiplier = 1.1f;

    [SerializeField]
    private Color _highlightColor = new(1f, 0.95f, 0.6f, 1f);

    [SerializeField, Self]
    private SpriteRenderer _spriteRenderer;

    [SerializeField, Self]
    private Graphic _graphic;

    private Vector3 _baseScale;
    private Color _baseColor;
    private bool _baseColorCaptured;
    private object _runtimeContext;

    public TutorialTargetId Id { get; private set; }
    public object Context => _runtimeContext;

    private void Awake()
    {
        _baseScale = transform.localScale;

        if (_marker != null)
            _marker.SetActive(false);

        if (_spriteRenderer != null)
        {
            _baseColor = _spriteRenderer.color;
            _baseColorCaptured = true;
        }
        else if (_graphic != null)
        {
            _baseColor = _graphic.color;
            _baseColorCaptured = true;
        }
    }

    private void OnDestroy()
    {
        Hide();
    }

    public void Configure(TutorialTargetId id, object context = null)
    {
        Id = id;
        _runtimeContext = context;
    }

    public bool Matches(TutorialTargetId id, object context)
    {
        if (Id != id)
            return false;

        if (context == null)
            return true;

        return Equals(_runtimeContext, context);
    }

    public void Show()
    {
        if (_marker != null)
        {
            _marker.SetActive(true);
            return;
        }

        transform.localScale = _baseScale * _highlightScaleMultiplier;
        SetColor(_highlightColor);
    }

    public void Hide()
    {
        if (_marker != null)
            _marker.SetActive(false);

        transform.localScale = _baseScale;

        if (_baseColorCaptured)
            SetColor(_baseColor);
    }

    private void SetColor(Color color)
    {
        if (_spriteRenderer != null)
            _spriteRenderer.color = color;

        if (_graphic != null)
            _graphic.color = color;
    }
}
