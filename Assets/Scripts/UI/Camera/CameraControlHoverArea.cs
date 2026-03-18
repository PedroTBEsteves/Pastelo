using System;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraControlHoverArea : ValidatedMonoBehaviour, IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler
{
    [SerializeField]
    private Direction _direction;
    
    [Inject]
    private readonly CameraController _cameraController;

    private void SetCameraMovePercentage(PointerEventData eventData)
    {
        switch (_direction)
        {
            case Direction.Left:
                _cameraController.GoToPreviousSection();
                break;
            case Direction.Right:
                _cameraController.GoToNextSection();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData) => SetCameraMovePercentage(eventData);

    public void OnPointerMove(PointerEventData eventData) => SetCameraMovePercentage(eventData);

    public void OnPointerExit(PointerEventData eventData) => _cameraController.StopMoving();

    [Serializable]
    private enum Direction
    {
        Left,
        Right,
    }
}
