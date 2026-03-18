using System;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using UnityEngine;

public class CameraController
{
    private readonly Camera _currentCamera;
    private readonly Dictionary<CameraSection, Vector3> _sectionPositions;
    private readonly CameraSection[] _orderedSections;
    private readonly float _transitionDuration;
    private readonly Ease _transitionEase;
    private readonly SectionController _sectionController;
    
    private Tween _transitionTween;
    private int _currentSectionIndex;
    private int _queuedDirection;
    private bool _isMoving;
    
    public CameraController(IReadOnlyDictionary<CameraSection, Vector3> sectionPositions, float transitionDuration, Ease transitionEase, SectionController sectionController)
    {
        _currentCamera = Camera.main;
        _sectionPositions = new Dictionary<CameraSection, Vector3>(sectionPositions);
        _orderedSections = _sectionPositions
            .OrderBy(pair => pair.Value.x)
            .Select(pair => pair.Key)
            .ToArray();
        _transitionDuration = transitionDuration;
        _transitionEase = transitionEase;
        _sectionController = sectionController;
        _currentSectionIndex = GetNearestSectionIndex(_currentCamera.transform.position);
    }
    
    public event Action CameraBeganMoving = delegate { };
    public event Action CameraEndedMoving = delegate { };
    public event Action CameraMoved = delegate { };
    public event Action<CameraSection> CurrentSectionChanged = delegate { };

    public CameraSection CurrentSection => _orderedSections.Length == 0 ? default : _orderedSections[_currentSectionIndex];

    private Rect GetViewRect()
    {
        var height = _currentCamera.orthographicSize * 2;
        var width = _currentCamera.aspect * height;
        var size = new Vector3(width, height);
        var position = _currentCamera.transform.position - size / 2;
        return new Rect(position, size);
    }

    public void StopMoving()
    {
        _queuedDirection = 0;
    }

    public CameraSection GetSectionInDirection(int direction)
    {
        if (_orderedSections.Length == 0)
            return default;

        var targetIndex = WrapIndex(_currentSectionIndex + Mathf.Clamp(direction, -1, 1));
        return _orderedSections[targetIndex];
    }

    public void GoImmediatelyToSection(CameraSection section)
    {
        var wasMoving = _isMoving;
        _transitionTween.Stop();
        _isMoving = false;
        _queuedDirection = 0;
        _currentSectionIndex = GetSectionIndex(section);
        _currentCamera.transform.position = _sectionPositions[section];
        SyncSectionsToCamera();
        CameraMoved();
        CurrentSectionChanged(CurrentSection);
        if (wasMoving)
            CameraEndedMoving();
    }

    public void GoToNextSection() => QueueDirection(1);

    public void GoToPreviousSection() => QueueDirection(-1);
    
    public Vector2 ScreenToWorldPointy(Vector2 screenPosition) => _currentCamera.ScreenToWorldPoint(screenPosition);

    private void QueueDirection(int direction)
    {
        if (direction == 0 || _orderedSections.Length == 0)
            return;

        _queuedDirection = Mathf.Clamp(direction, -1, 1);
        if (_isMoving)
            return;

        StartTransition(_queuedDirection);
    }

    private void StartTransition(int direction)
    {
        if (_orderedSections.Length == 0)
            return;

        _queuedDirection = Mathf.Clamp(direction, -1, 1);
        _currentSectionIndex = GetNearestSectionIndex(_currentCamera.transform.position);
        var targetIndex = WrapIndex(_currentSectionIndex + _queuedDirection);
        var targetSection = _orderedSections[targetIndex];
        var startPosition = _currentCamera.transform.position;
        var targetPosition = _queuedDirection > 0
            ? _sectionController.GetNextSectionPosition(startPosition.x)
            : _sectionController.GetPreviousSectionPosition(startPosition.x);
        targetPosition.z = _currentCamera.transform.position.z;
        _sectionPositions[targetSection] = targetPosition;
        var duration = Mathf.Max(0f, _transitionDuration);

        if (duration <= 0f)
        {
            _currentSectionIndex = targetIndex;
            _currentCamera.transform.position = targetPosition;
            SyncSectionsToCamera();
            CameraMoved();
            CurrentSectionChanged(CurrentSection);
            TryContinueQueuedMovement();
            return;
        }

        _isMoving = true;
        CameraBeganMoving();
        _transitionTween.Stop();
        _transitionTween = Tween.Position(_currentCamera.transform, startPosition, targetPosition, duration, _transitionEase)
            .OnUpdate(this, static (controller, _) =>
            {
                controller.SyncSectionsToCamera();
                controller.CameraMoved();
            })
            .OnComplete(this, controller =>
            {
                controller._isMoving = false;
                controller._currentSectionIndex = targetIndex;
                controller.SyncSectionsToCamera();
                controller.CameraMoved();
                controller.CurrentSectionChanged(controller.CurrentSection);
                controller.CameraEndedMoving();
                controller.TryContinueQueuedMovement();
            });
    }

    private void TryContinueQueuedMovement()
    {
        if (_queuedDirection == 0)
            return;

        StartTransition(_queuedDirection);
    }

    private int GetNearestSectionIndex(Vector3 position)
    {
        if (_orderedSections.Length == 0)
            return 0;

        var nearestIndex = 0;
        var nearestDistance = float.MaxValue;

        for (var i = 0; i < _orderedSections.Length; i++)
        {
            var section = _orderedSections[i];
            var distance = Mathf.Abs(_sectionPositions[section].x - position.x);
            if (distance >= nearestDistance)
                continue;

            nearestDistance = distance;
            nearestIndex = i;
        }

        return nearestIndex;
    }

    private int GetSectionIndex(CameraSection section)
    {
        for (var i = 0; i < _orderedSections.Length; i++)
        {
            if (_orderedSections[i] == section)
                return i;
        }

        return 0;
    }

    private int WrapIndex(int index)
    {
        if (_orderedSections.Length == 0)
            return 0;

        return (index % _orderedSections.Length + _orderedSections.Length) % _orderedSections.Length;
    }

    private void SyncSectionsToCamera() => _sectionController.SyncToViewRect(GetViewRect());
}
