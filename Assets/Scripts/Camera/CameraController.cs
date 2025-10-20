using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class CameraController
{
    private readonly Camera _currentCamera;
    private readonly IReadOnlyDictionary<CameraSection, Vector3> _sectionPositions;
    private readonly TweenSettings _transitionSettings;
    private readonly AudioSource _transitionSound;
    
    private CameraSection _currentSection;
    private Tween _transitionTween;
    
    public CameraController(IReadOnlyDictionary<CameraSection, Vector3> sectionPositions, TweenSettings transitionSettings, AudioSource transitionSound)
    {
        _currentCamera = Camera.main;
        _sectionPositions = new Dictionary<CameraSection, Vector3>(sectionPositions);
        _transitionSettings = transitionSettings;
        _transitionSound = transitionSound;
    }

    public event Action SectionTransitionStarted = delegate { };
    public event Action SectionTransitionFinished = delegate { };
    
    public void GoToNextSection()
    {
        if (_transitionTween.isAlive)
            return;
        
        _currentSection++;
        
        if (!Enum.IsDefined(typeof(CameraSection), _currentSection))
            _currentSection = CameraSection.Balcony;
        
        TransitionToSection();
    }

    public void GoToPreviousSection()
    {
        if (_transitionTween.isAlive)
            return;
        
        _currentSection--;
        
        if (!Enum.IsDefined(typeof(CameraSection), _currentSection))
            _currentSection = CameraSection.Packing;
        
        TransitionToSection();
    }

    public void GoImmediatelyToSection(CameraSection section)
    {
        _currentSection = section;
        _currentCamera.transform.position = _sectionPositions[section];
    }

    private void TransitionToSection()
    {
        SectionTransitionStarted();
        var position = _sectionPositions[_currentSection];
        _transitionTween = Tween.Position(_currentCamera.transform, position, _transitionSettings)
            .OnComplete(this, controller => controller.SectionTransitionFinished());
        _transitionSound.Play();
    }
    
    public Vector2 ScreenToWorldPointy(Vector2 screenPosition) => _currentCamera.ScreenToWorldPoint(screenPosition);
}
