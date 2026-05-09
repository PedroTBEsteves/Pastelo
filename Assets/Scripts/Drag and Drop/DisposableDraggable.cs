using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IDiscardPolicy
{
    bool CanBeDiscarded();
}

public interface IDiscardHandler
{
    void Discard();
}

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

    private void OnDropped(PointerEventData eventData)
    {
        if (!CanBeDiscarded())
        {
            _trashBin.Hide();
            return;
        }

        _trashBin.TryDiscard(_draggable, eventData);
    }

    private void OnHeld(PointerEventData _)
    {
        if (!CanBeDiscarded())
            return;

        _trashBin.Show();
    }

    private bool CanBeDiscarded()
    {
        var discardPolicy = GetComponent(typeof(IDiscardPolicy)) as IDiscardPolicy;
        return discardPolicy?.CanBeDiscarded() ?? true;
    }

    public bool TryGetDiscardHandler(out IDiscardHandler discardHandler)
    {
        discardHandler = GetComponent(typeof(IDiscardHandler)) as IDiscardHandler;
        return discardHandler != null;
    }
}
