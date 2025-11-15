using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class PopupTextService : MonoBehaviour, IPopupTextService
{
    [SerializeField]
    private TextMeshProUGUI _popupPrefab;

    [SerializeField]
    private float _movePositionY;
    
    [SerializeField]
    private TweenSettings _moveTweenSettings;
    
    [SerializeField]
    private TweenSettings<float> _alphaTweenSettings;

    [SerializeField]
    private Color _errorColor;
    
    private ObjectPool<TextMeshProUGUI> _textPool;

    private void Awake()
    {
        _textPool = new ObjectPool<TextMeshProUGUI>(
            CreatePopup,
            text => text.gameObject.SetActive(true),
            text => text.gameObject.SetActive(false));
    }
    
    private TextMeshProUGUI CreatePopup() => Instantiate(_popupPrefab, transform);
    
    public void ShowError(string message, Vector3 position)
    {
        var text = _textPool.Get();
        text.SetText(message);
        text.color = _errorColor;
        text.transform.position = position;
        Sequence.Create()
            .Group(Tween.Alpha(text, _alphaTweenSettings))
            .Group(Tween.Position(text.transform, position, position + Vector3.up * _movePositionY, _moveTweenSettings))
            .OnComplete(text, _textPool.Release);
    }
}
