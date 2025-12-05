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

    public void SetVelocityPercentage(float percentage)
    {
        if (_currentSpeed == 0)
            CameraBeganMoving();
        
        _currentSpeed = Mathf.LerpUnclamped(0,  _movementSpeed, percentage);
    }

    public void StopMoving()
    {
        _currentSpeed = 0;
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
        if (_currentSpeed == 0) 
            return;
        
        _currentCamera.transform.Translate(_currentSpeed * deltaTime * Vector3.right);
        CameraMoved();
    }
}
