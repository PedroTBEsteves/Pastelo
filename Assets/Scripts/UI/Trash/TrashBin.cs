using System;
using System.Collections.Generic;
using System.Linq;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TrashBin : MonoBehaviour
{
    [SerializeField]
    private GraphicRaycaster _raycaster;

    [SerializeField]
    private AudioSource _audioSource;

    [SerializeField]
    private Image _image;

    [Inject]
    private readonly GameplayInteractionGate _interactionGate;
    
    private List<RaycastResult> _raycastResults = new();

    private void Awake()
    {
        _image.enabled = false;
    }

    public void Show()
    {
        if (!_interactionGate.CanInteract(TutorialInteractionType.DiscardItem))
            return;

        _image.enabled = true;
    }
    
    public void TryDiscard(Draggable draggable, PointerEventData eventData)
    {
        if (!_interactionGate.CanInteract(TutorialInteractionType.DiscardItem))
        {
            Hide();
            return;
        }

        if (IsInside(eventData))
        {
            Destroy(draggable.gameObject);
            PlayDiscardSound();
        }
        
        Hide();
    }
    
    private bool IsInside(PointerEventData eventData)
    {
        _raycaster.Raycast(eventData, _raycastResults);

        var containsTrashBin = _raycastResults.Any(result => result.gameObject == gameObject);
        
        _raycastResults.Clear();
        
        return containsTrashBin;
    }

    private void PlayDiscardSound() => _audioSource.Play();

    private void Hide()
    {
        _image.enabled = false;
    }
}
