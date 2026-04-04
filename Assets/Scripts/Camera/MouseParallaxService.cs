using UnityEngine;
using UnityEngine.InputSystem;

public class MouseParallaxService : ITickable
{
    private readonly Vector2 _maxOffset;
    private readonly float _smoothTime;

    private Vector2 _currentOffset;
    private Vector2 _offsetVelocity;

    public MouseParallaxService(Vector2 maxOffset, float smoothTime)
    {
        _maxOffset = maxOffset;
        _smoothTime = smoothTime;
    }

    public Vector2 CurrentOffset => _currentOffset;

    public void Tick(float deltaTime)
    {
        var targetOffset = CalculateTargetOffset();
        if (_smoothTime <= 0f)
        {
            _currentOffset = targetOffset;
            _offsetVelocity = Vector2.zero;
            return;
        }

        _currentOffset = Vector2.SmoothDamp(_currentOffset, targetOffset, ref _offsetVelocity, _smoothTime, Mathf.Infinity, deltaTime);
    }

    private Vector2 CalculateTargetOffset()
    {
        if (Pointer.current == null)
            return Vector2.zero;

        var screenWidth = Screen.width;
        var screenHeight = Screen.height;
        if (screenWidth <= 0 || screenHeight <= 0)
            return Vector2.zero;

        var pointerPosition = Pointer.current.position.ReadValue();
        var normalizedPosition = new Vector2(
            Mathf.Clamp(pointerPosition.x / screenWidth, 0f, 1f) * 2f - 1f,
            Mathf.Clamp(pointerPosition.y / screenHeight, 0f, 1f) * 2f - 1f);

        return Vector2.Scale(normalizedPosition, _maxOffset);
    }
}
