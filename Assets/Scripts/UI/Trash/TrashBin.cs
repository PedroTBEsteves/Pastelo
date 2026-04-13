using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TrashBin : MonoBehaviour
{
    [SerializeField]
    private AudioSource _audioSource;

    [SerializeField]
    private Image _image;

    [Inject]
    private readonly GameplayInteractionGate _interactionGate;
    
    private void Awake()
    {
        _image.enabled = false;
        _image.raycastTarget = false;
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
        var eventCamera = eventData.pressEventCamera != null
            ? eventData.pressEventCamera
            : eventData.enterEventCamera;

        return RectTransformUtility.RectangleContainsScreenPoint(_image.rectTransform, eventData.position, eventCamera);
    }

    private void PlayDiscardSound() => _audioSource.Play();

    public void Hide()
    {
        _image.enabled = false;
    }
}
