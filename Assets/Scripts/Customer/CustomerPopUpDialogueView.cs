using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomerPopUpDialogueView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private TextMeshProUGUI _text;

    [SerializeField]
    private Image _customerImage;

    [SerializeField]
    private AudioSource _audioSource;

    public TextMeshProUGUI Text => _text;
    public Image CustomerImage => _customerImage;
    public AudioSource AudioSource => _audioSource;
    public event Action Clicked;

    public void OnPointerClick(PointerEventData eventData)
    {
        Clicked?.Invoke();
    }
}
