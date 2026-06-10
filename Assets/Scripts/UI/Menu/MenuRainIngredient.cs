using System;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public sealed class MenuRainIngredient : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private RectTransform _rectTransform;

    [SerializeField, Self]
    private Image _image;

    private Action<MenuRainIngredient> _release;
    private Vector2 _velocity;
    private float _angularSpeed;
    private float _lifetime;
    private float _elapsedTime;
    private bool _isRunning;

    public void Initialize(
        Sprite sprite,
        Vector2 anchoredPosition,
        Vector2 velocity,
        float angularSpeed,
        float lifetime,
        Action<MenuRainIngredient> release)
    {
        _image.sprite = sprite;
        _image.enabled = true;
        _image.SetNativeSize();
        _rectTransform.anchoredPosition = anchoredPosition;
        _rectTransform.localRotation = Quaternion.identity;

        _velocity = velocity;
        _angularSpeed = angularSpeed;
        _lifetime = lifetime;
        _release = release;
        _elapsedTime = 0f;
        _isRunning = true;
    }

    public void StopForPool()
    {
        _isRunning = false;
        _release = null;
        _image.sprite = null;
        _image.enabled = false;
    }

    private void Update()
    {
        if (!_isRunning)
            return;

        var deltaTime = Time.deltaTime;
        _elapsedTime += deltaTime;

        _rectTransform.anchoredPosition += _velocity * deltaTime;
        _rectTransform.Rotate(0f, 0f, _angularSpeed * deltaTime);

        if (_elapsedTime < _lifetime)
            return;

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (!_isRunning)
            return;

        _isRunning = false;
        var release = _release;
        _release = null;
        release?.Invoke(this);
    }
}
