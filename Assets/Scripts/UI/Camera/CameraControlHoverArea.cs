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

    private void Awake()
    {
        _tutorialTarget = GetComponent<TutorialTarget>() ?? gameObject.AddComponent<TutorialTarget>();
        _tutorialTarget.Configure(_direction == Direction.Left ? TutorialTargetId.CameraMoveLeft : TutorialTargetId.CameraMoveRight);
        _tutorialTargetRegistry.Register(_tutorialTarget);
    }

    private void OnDestroy()
    {
        _tutorialTargetRegistry.Unregister(_tutorialTarget);
    }

    private void SetCameraMovePercentage(PointerEventData eventData)
    {
        var directionValue = _direction == Direction.Left ? -1 : 1;
        var targetSection = _cameraController.GetSectionInDirection(directionValue);

        if (!_interactionGate.CanInteract(TutorialInteractionType.MoveCamera, targetSection))
            return;

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
