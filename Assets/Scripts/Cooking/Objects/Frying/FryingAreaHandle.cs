using KBCore.Refs;
using UnityEngine;
using UnityEngine.EventSystems;

public class FryingAreaHandle : ValidatedMonoBehaviour, IPointerDownHandler
{
    [SerializeField, Scene]
    private FryingArea _fryingArea;
    
    public void OnPointerDown(PointerEventData eventData)
    {
        _fryingArea.ToggleRaised();
    }
}
