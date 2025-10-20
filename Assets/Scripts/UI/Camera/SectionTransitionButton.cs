using System;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SectionTransitionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private bool _previous;
    
    [Inject]
    private readonly CameraController _cameraController;

    [Inject]
    private readonly ICustomerDialogue _customerDialogue;

    private bool _pointeIsInside;

    private void Awake()
    {
        _cameraController.SectionTransitionFinished += OnSectionTransitionFinished;
    }

    private void Transition()
    {
        if (_customerDialogue.IsPlaying)
            return;
        
        if (_previous)
            _cameraController.GoToPreviousSection();
        else
            _cameraController.GoToNextSection();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Transition();
        _pointeIsInside = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _pointeIsInside = false;
    }

    private void OnSectionTransitionFinished()
    {
        if (_pointeIsInside)
            Transition();
    }
}
