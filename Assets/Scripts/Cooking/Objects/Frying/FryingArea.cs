using System;
using System.Linq;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.VFX;

public class FryingArea : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private BoxCollider2D _collider;
    
    [SerializeField, Child(Flag.ExcludeSelf)]
    private SpriteRenderer _spriteRenderer;
    
    [SerializeField]
    private Transform _discardPositionTransform;

    [SerializeField, Self]
    private AudioSource _fryingSource;

    [SerializeField, Child(Flag.ExcludeSelf)]
    private AudioSource _stoveSound;
    
    [SerializeField]
    private Sprite _offSprite;
    
    [SerializeField]
    private Sprite _onSprite;
    
    [SerializeField]
    private Transform _heightTransform;
    
    [SerializeField]
    private VisualEffect[] _visualEffects;
    
    private readonly DraggableClosedPastel[] _fryingPastels = new DraggableClosedPastel[4];
    
    public Vector3 DiscardPosition => _discardPositionTransform.position;
    
    public bool TryAdd(DraggableClosedPastel draggableClosedPastel, Vector3 position)
    {
        if (_fryingPastels.All(closed => closed != null))
            return false;

        if (_fryingPastels.All(closed => closed == null))
            StartFrying();
        
        var index = GetNearestSlotIndex(position);
        var newPosition = GetPositionForSlot(index);
        draggableClosedPastel.transform.position = newPosition;
        _fryingPastels[index] = draggableClosedPastel;
        return true;
    }
    
    public void Remove(DraggableClosedPastel draggableClosedPastel)
    {
        var index = Array.IndexOf(_fryingPastels, draggableClosedPastel);

        if (index != -1)
        {
            _fryingPastels[index] = null;
            
            if (_fryingPastels.All(pastel => pastel == null))
                StopFrying();
        }
    }

    private void StartFrying()
    {
        _fryingSource.Play();
        _stoveSound.Play();
        foreach (var visualEffect in _visualEffects)
            visualEffect.Play();
        
        _spriteRenderer.sprite = _onSprite;
    }

    private void StopFrying()
    {
        _fryingSource.Stop();
        foreach (var visualEffect in _visualEffects)
            visualEffect.Stop();
        
        _spriteRenderer.sprite = _offSprite;
    }

    private int GetNearestSlotIndex(Vector3 position)
    {
        return Array.IndexOf(_fryingPastels, null);
    }

    private Vector3 GetPositionForSlot(int index)
    {
        var size = _collider.size.x * 0.7f;

        var offset = new Vector3(size/4 * (index - 2) + size/8, 0, -1);
        return transform.position + offset;
    }

    private void Update()
    {
        foreach (var pastel in _fryingPastels.Where(pastel => pastel != null))
            pastel.Fry(Time.deltaTime);
    }
}
