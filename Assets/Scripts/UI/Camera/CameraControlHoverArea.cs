using System;
using KBCore.Refs;
using PrimeTween;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraControlHoverArea : ValidatedMonoBehaviour, IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler
{
    [SerializeField, Self]
    private RectTransform _rectTransform;

    [SerializeField]
    private Direction _direction;
    
    [Inject]
    private readonly CameraController _cameraController;

    private const EasingFunction.Ease EASE = EasingFunction.Ease.EaseOutSine;

    private void SetCameraMovePercentage(PointerEventData eventData)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rectTransform,
                eventData.position,
                eventData.enterEventCamera,
                out var position))
            return;
        
        var direction = _direction == Direction.Left ? -1 : 1;
        var percentage = Rect.PointToNormalized(_rectTransform.rect, position).x;
        percentage = EasingFunction.Evaluate(0, 1, percentage, EASE);
        _cameraController.SetVelocityPercentage(percentage * direction);
    }
    
    public void OnPointerEnter(PointerEventData eventData) => SetCameraMovePercentage(eventData);

    public void OnPointerMove(PointerEventData eventData) => SetCameraMovePercentage(eventData);

    public void OnPointerExit(PointerEventData eventData) =>  _cameraController.StopMoving();

    [Serializable]
    private enum Direction
    {
        Left,
        Right,
    }
}