using System.Collections.Generic;
using System.Linq;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Draggable))]
public class BumpableDraggable : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private Draggable _draggable;

    [SerializeField, Self]
    private Collider2D _collider;
    
    private List<Collider2D> _overlapColliders = new();
    
    private void Awake()
    {
        _draggable.Dropped += OnDropped;
    }

    private void OnDestroy()
    {
        _draggable.Dropped -= OnDropped;
    }

    private void OnDropped(PointerEventData obj)
    {
        _overlapColliders.Clear();
        var hits = _collider.Overlap(_overlapColliders);
        
        if (hits == 0)
            return;

        var bumper = _overlapColliders
            .Select(collider => collider.GetComponent<DraggableBumper>())
            .Where(bumper => bumper != null)
            .OrderBy(bumper => Vector2.Distance(bumper.transform.position, transform.position))
            .First();
        
        bumper.Bump(transform);
    }
}
