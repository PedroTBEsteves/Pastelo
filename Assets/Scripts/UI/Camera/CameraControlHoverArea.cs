using System;
using KBCore.Refs;
using PrimeTween;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraControlHoverArea : ValidatedMonoBehaviour, IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler
{
    [SerializeField]
    private Direction _direction;

    [SerializeField]
    [Min(0f)]
    private float _repeatMoveDelay = 0.25f;
    
    [Inject]
    private readonly CameraController _cameraController;

    [Inject]
    private readonly GameplayInteractionGate _interactionGate;

    [Inject]
    private readonly TutorialTargetRegistry _tutorialTargetRegistry;

    private TutorialTarget _tutorialTarget;
    private bool _isPointerHovering;
    private bool _hasTriggeredMoveThisHover;
    private bool _isRepeatMoveDelayActive;
    private Tween _repeatMoveDelayTween;

    private void Awake()
    {
        _tutorialTarget = GetComponent<TutorialTarget>() ?? gameObject.AddComponent<TutorialTarget>();
        _tutorialTarget.Configure(_direction == Direction.Left ? TutorialTargetId.CameraMoveLeft : TutorialTargetId.CameraMoveRight);
        _tutorialTargetRegistry.Register(_tutorialTarget);
        _cameraController.CameraEndedMoving += OnCameraEndedMoving;
    }

    private void OnDestroy()
    {
        CancelRepeatMoveDelay();
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
        _cameraController.StopMoving();

        if (!_isPointerHovering || !_hasTriggeredMoveThisHover)
            return;

        ScheduleRepeatMove();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        _isPointerHovering = true;

        if (_hasTriggeredMoveThisHover)
            return;

        CancelRepeatMoveDelay();
        _hasTriggeredMoveThisHover = true;
        SetCameraMovePercentage(eventData);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!_isPointerHovering)
            return;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isPointerHovering = false;
        _hasTriggeredMoveThisHover = false;
        CancelRepeatMoveDelay();
        _cameraController.StopMoving();
    }

    private void ScheduleRepeatMove()
    {
        if (_isRepeatMoveDelayActive)
            return;

        var delay = Mathf.Max(0f, _repeatMoveDelay);
        if (delay <= 0f)
        {
            TryRepeatMove();
            return;
        }

        _isRepeatMoveDelayActive = true;
        _repeatMoveDelayTween = Tween.Delay(delay, TryRepeatMove);
    }

    private void TryRepeatMove()
    {
        _isRepeatMoveDelayActive = false;

        if (!_isPointerHovering)
            return;

        SetCameraMovePercentage(null);
    }

    private void CancelRepeatMoveDelay()
    {
        _isRepeatMoveDelayActive = false;

        if (_repeatMoveDelayTween.isAlive)
            _repeatMoveDelayTween.Stop();
    }

    [Serializable]
    private enum Direction
    {
        Left,
        Right,
    }
}
