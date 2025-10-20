using System;
using System.Linq;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.EventSystems;

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
    
    private readonly DraggableClosedPastel[] _fryingPastels = new DraggableClosedPastel[4];

    private bool _raised = true;

    public Vector3 DiscardPosition => _discardPositionTransform.position;
    
    public void ToggleRaised()
    {
        foreach (var fryingPastel in _fryingPastels)
            fryingPastel?.ToggleRaised();
        
        _raised = !_raised;

        if (!_raised)
        {
            _fryingSource.Play();
            _stoveSound.Play();
        }
        else
            _fryingSource.Stop();
        
        _spriteRenderer.sprite = _raised ? _offSprite : _onSprite;
    }
    
    public bool TryAdd(DraggableClosedPastel draggableClosedPastel, Vector3 position)
    {
        if (!_raised || _fryingPastels.All(closed => closed != null))
            return false;

        var index = GetNearestSlotIndex(position);
        var newPosition = GetPositionForSlot(index);
        draggableClosedPastel.transform.position = newPosition;
        _fryingPastels[index] = draggableClosedPastel;
        return true;
    }

    private int GetNearestSlotIndex(Vector3 position)
    {
        // var distanceToCenter = position - transform.position;
        // Debug.Log(distanceToCenter);
        //
        // var index = (distanceToCenter.x <= 0 ? 0 : 1) + (distanceToCenter.y >= 0 ? 0 : 2);
        // Debug.Log(index);
        //
        // if (_fryingPastels[index] == null)
        //     return index;
        //
        // var offset = index % 2 == 0 ? 1 : -1;
        // index += offset;
        //
        // if (_fryingPastels[index] == null)
        //     return index;
        //
        // index -= offset;
        // offset = index < 2 ? 2 : -2;
        // index += offset;
        //
        // if (_fryingPastels[index] == null)
        //     return index;
        //
        // offset = index % 2 == 0 ? 1 : -1;
        // index += offset;
        //
        // return index;

        return Array.IndexOf(_fryingPastels, null);
    }

    private Vector3 GetPositionForSlot(int index)
    {
        // var xOffset = _collider.size.x / 4;
        // var yOffset = _collider.size.y / 4;
        //
        // var offset = new Vector3(index % 2 == 0 ? -xOffset : xOffset, index < 2 ? yOffset : -yOffset, -1);
        //
        // return transform.position + offset;

        var size = _collider.size.x * 0.7f;

        var offset = new Vector3(size/4 * (index - 2) + size/8, 0, -1);
        return transform.position + offset;
    }

    public void Remove(DraggableClosedPastel draggableClosedPastel)
    {
        var index = Array.IndexOf(_fryingPastels, draggableClosedPastel);
        
        if (index != -1)
            _fryingPastels[index] = null;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        ToggleRaised();
    }
}
