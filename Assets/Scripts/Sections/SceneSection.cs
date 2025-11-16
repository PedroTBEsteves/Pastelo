using System;
using KBCore.Refs;
using UnityEngine;

public class SceneSection : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private BoxCollider2D _sizeCollider;

    [field: SerializeField]
    public SceneSection Next { get; private set; }
    
    [field: SerializeField]
    public SceneSection Previous { get; private set; }
    
    private float _size;

    private void Awake()
    {
        _size = _sizeCollider.size.x - 0.005f;
        Destroy(_sizeCollider);
    }

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

    public bool IsInside(float position) => position >= GetXMin() && position <= GetXMax();
    
    private float GetHalfWidth() => _size / 2;
    
    protected override void OnValidate()
    {
        base.OnValidate();
        if (Next != null)
            Next.Previous = this;
        if (Previous != null)
            Previous.Next = this;
    }
}
