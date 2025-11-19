using System;
using KBCore.Refs;
using Reflex.Attributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Draggable))]
public class SectionAttachedDraggable : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private Draggable _draggable;

    [Inject]
    private readonly SectionController _sectionController;
    
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

    private void OnHeld(PointerEventData _) => transform.parent = null;

    private void OnDropped(PointerEventData _) => _sectionController.AttachToSection(transform);
}
