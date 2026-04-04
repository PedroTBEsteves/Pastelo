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

    [Inject]
    private readonly GameplayInteractionGate _interactionGate;

    [Inject]
    private readonly TutorialTargetRegistry _tutorialTargetRegistry;

    private TutorialTarget _tutorialTarget;
    private bool _isPointerHovering;

    private void Awake()
    {
        _tutorialTarget = GetComponent<TutorialTarget>() ?? gameObject.AddComponent<TutorialTarget>();
        _tutorialTarget.Configure(_direction == Direction.Left ? TutorialTargetId.CameraMoveLeft : TutorialTargetId.CameraMoveRight);
        _tutorialTargetRegistry.Register(_tutorialTarget);
        _cameraController.CameraEndedMoving += OnCameraEndedMoving;
    }

    private void OnDestroy()
    {
        _cameraController.CameraEndedMoving -= OnCameraEndedMoving;
        _tutorialTargetRegistry.Unregister(_tutorialTarget);
    }

    private void SetCameraMovePercentage(PointerEventData _)
    {
        var directionValue = _direction == Direction.Left ? -1 : 1;
        var targetSection = _cameraController.GetSectionInDirection(directionValue);

        if (!_interactionGate.CanInteract(TutorialInteractionType.MoveCamera, targetSection))
        {
            _cameraController.StopMoving();
            return;
        }

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

    private void OnCameraEndedMoving()
    {
        if (!_isPointerHovering)
            return;

        SetCameraMovePercentage(null);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        _isPointerHovering = true;
        SetCameraMovePercentage(eventData);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!_isPointerHovering)
            return;

        SetCameraMovePercentage(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isPointerHovering = false;
        _cameraController.StopMoving();
    }

    [Serializable]
    private enum Direction
    {
        Left,
        Right,
    }
}
