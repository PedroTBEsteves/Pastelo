using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DialoguePresentationView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private TextMeshProUGUI _text;

    public TextMeshProUGUI Text => _text;
    public event Action Clicked;

    public void OnPointerClick(PointerEventData eventData)
    {
        Clicked?.Invoke();
    }
}
