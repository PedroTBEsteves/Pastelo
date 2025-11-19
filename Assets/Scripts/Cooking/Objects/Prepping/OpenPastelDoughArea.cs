using System.Collections.Generic;
using KBCore.Refs;
using PrimeTween;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

public class OpenPastelDoughArea : ValidatedMonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField, Self]
    private SpriteRenderer _spriteRenderer;

    [SerializeField]
    private float _pressDuration;

    [SerializeField]
    private DraggableClosedPastel _closedPastelPrefab;
    
    [Inject]
    private readonly PastelCookingSettings _pastelCookingSettings;
    
    [Inject]
    private readonly CameraController _cameraController;
    
    private OpenPastelDough _pastel;

    private Tween _pressTween;
    
    private List<DraggableFilling> _fillings = new List<DraggableFilling>();
    
    public int FillingsCount => _fillings.Count;
    
    public bool TryOpenDough(Dough dough)
    {
        if (_pastel != null)
            return false;
        
        _spriteRenderer.sprite = dough.OpenDoughSprite;
        _pastel = new OpenPastelDough(dough);

        return true;
    }

    public bool TryAddFilling(DraggableFilling filling)
    {
        if (_pastel == null)
            return false;
        
        _fillings.Add(filling);
        _pastel.AddFilling(filling.Ingredient);
        return true;
    }

    private void Close()
    {
        if (_pastel == null)
            return;
        
        var closedPastel = _pastel.Close(_pastelCookingSettings);
        _pastel = null;
        _spriteRenderer.sprite = null;
        var draggableClosedPastel = Instantiate(_closedPastelPrefab, transform.position + Vector3.forward, Quaternion.identity, transform.parent);
        draggableClosedPastel.Initialize(closedPastel);
        foreach (var fillings in _fillings)
            Destroy(fillings.gameObject);
        _fillings.Clear();
        //_cameraController.GoToNextSection();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _pressTween = Tween.Delay(_pressDuration, Close);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_pressTween.isAlive)
            _pressTween.Stop();
    }
}
