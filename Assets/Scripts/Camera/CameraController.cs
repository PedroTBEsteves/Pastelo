using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class CameraController : ITickable
{
    private readonly Camera _currentCamera;
    private readonly IReadOnlyDictionary<CameraSection, Vector3> _sectionPositions;
    private readonly float _movementSpeed;
    private readonly float _acceleration;
    
    private Tween _transitionTween;
    private bool _moving;
    private bool _right;
    private float _currentSpeed;
    
    public CameraController(IReadOnlyDictionary<CameraSection, Vector3> sectionPositions, float movementSpeed, float acceleration)
    {
        _currentCamera = Camera.main;
        _sectionPositions = new Dictionary<CameraSection, Vector3>(sectionPositions);
        _movementSpeed = movementSpeed;
        _acceleration = acceleration;
    }
    
    public event Action CameraBeganMoving = delegate { };
    public event Action CameraEndedMoving = delegate { };
    public event Action CameraMoved = delegate { };

    public Rect GetViewRect()
    {
        var height = _currentCamera.orthographicSize * 2;
        var width = _currentCamera.aspect * height;
        var size = new Vector3(width, height);
        var position = _currentCamera.transform.position - size / 2;
        return new Rect(position, size);
    }

    public void StartMoving(bool right)
    {
        _moving = true;
        _right = right;
        CameraBeganMoving();
    }

    public void StopMoving()
    {
        _moving = false;
        CameraEndedMoving();
    }

    public void GoImmediatelyToSection(CameraSection section)
    {
        _currentCamera.transform.position = _sectionPositions[section];
        CameraMoved();
    }
    
    public Vector2 ScreenToWorldPointy(Vector2 screenPosition) => _currentCamera.ScreenToWorldPoint(screenPosition);
    public void Tick(float deltaTime)
    {
        if (!_moving)
        {
            _currentSpeed = Mathf.Max(0, _currentSpeed - _acceleration * deltaTime);
        }
        else
        {
            _currentSpeed = Mathf.Min(_movementSpeed, _currentSpeed + _movementSpeed * deltaTime);
        }

        if (_currentSpeed == 0) 
            return;
        
        var direction = _right ? Vector3.right : Vector3.left;
        _currentCamera.transform.Translate(_currentSpeed * deltaTime * direction);
        CameraMoved();
    }
}
