using System;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Draggable))]
public class DisposableDraggable : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private Draggable _draggable;

    [Inject]
    private readonly TrashBin _trashBin;

    private void Awake()
    {
        _draggable.Held += OnHeld;
        _draggable.Dropped += OnDropped;
    }

    private void OnDestroy()
    {
        _draggable.Held -= OnHeld;
        _draggable.Dropped -= OnDropped;
    }

    private void OnHeld(PointerEventData _) => _trashBin.Show();

    private void OnDropped(PointerEventData eventData)
    {
        _trashBin.TryDiscard(_draggable, eventData);
    }
}
