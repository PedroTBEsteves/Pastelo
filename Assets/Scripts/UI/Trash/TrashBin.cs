using System.Collections.Generic;
using System.Linq;
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
    
    private List<RaycastResult> _raycastResults = new();
    
    public void Show()
    {
        _image.enabled = true;
    }
    
    public void TryDiscard(Draggable draggable, PointerEventData eventData)
    {
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
