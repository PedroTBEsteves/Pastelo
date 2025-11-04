using System;
using KBCore.Refs;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SceneSection : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private BoxCollider2D _sizeCollider;

    [field: SerializeField]
    public SceneSection Next { get; private set; }
    
    [field: SerializeField]
    public SceneSection Previous { get; private set; }

    public void MoveNext()
    {
        var position = transform.position;
        position.x += GetHalfWidth();
        position.x += Next.GetHalfWidth();
        Next.transform.position = position;
    }

    public void MovePrevious()
    {
        var position = transform.position;
        position.x -= GetHalfWidth();
        position.x -= Previous.GetHalfWidth();
        Previous.transform.position = position;
    }
    
    public float GetXMax() => transform.position.x + GetHalfWidth();
    
    public float GetXMin() => transform.position.x - GetHalfWidth();

    private float GetHalfWidth() => _sizeCollider.size.x / 2;
    
    protected override void OnValidate()
    {
        base.OnValidate();
        _sizeCollider.isTrigger = true;
        if (Next != null)
            Next.Previous = this;
        if (Previous != null)
            Previous.Next = this;
    }
}
