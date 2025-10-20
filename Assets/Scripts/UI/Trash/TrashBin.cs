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
    
    public bool IsInside(PointerEventData eventData)
    {
        _raycaster.Raycast(eventData, _raycastResults);

        return _raycastResults.Any(result => result.gameObject == gameObject);
    }

    public void PlayDiscardSound() => _audioSource.Play();
    
    public void Show()
    {
        _image.enabled = true;
    }

    public void Hide()
    {
        _image.enabled = false;
    }
}
