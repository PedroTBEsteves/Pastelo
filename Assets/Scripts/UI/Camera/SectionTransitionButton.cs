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
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        _cameraController.StartMoving(!_previous);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _cameraController.StopMoving();
    }
}
