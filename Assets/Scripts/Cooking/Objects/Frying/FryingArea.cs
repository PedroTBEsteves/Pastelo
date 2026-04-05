using System;
using System.Linq;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.VFX;

public class FryingArea : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private BoxCollider2D _collider;
    
    [SerializeField, Child(Flag.ExcludeSelf)]
    private Animator _animator;
    
    [SerializeField]
    private Transform _discardPositionTransform;

    [SerializeField, Self]
    private AudioSource _fryingSource;

    [SerializeField, Child(Flag.ExcludeSelf)]
    private AudioSource _stoveSound;
    
    [SerializeField]
    private Transform _heightTransform;
    
    [SerializeField]
    private VisualEffect[] _visualEffects;

    [Inject]
    private readonly GameplayTutorialEvents _tutorialEvents;

    [Inject]
    private readonly GameplayInteractionGate _interactionGate;

    [Inject]
    private readonly TutorialTargetRegistry _tutorialTargetRegistry;
    
    private readonly DraggableClosedPastel[] _fryingPastels = new DraggableClosedPastel[4];

    private TutorialTarget _tutorialTarget;
    
    public Vector3 DiscardPosition => _discardPositionTransform.position;

    private void Awake()
    {
        _tutorialTarget = GetComponent<TutorialTarget>() ?? gameObject.AddComponent<TutorialTarget>();
        _tutorialTarget.Configure(TutorialTargetId.FryingArea);
        _tutorialTargetRegistry.Register(_tutorialTarget);
    }

    private void OnDestroy()
    {
        _tutorialTargetRegistry.Unregister(_tutorialTarget);
    }
    
    public bool TryAdd(DraggableClosedPastel draggableClosedPastel, Vector3 position)
    {
        if (!_interactionGate.CanInteract(TutorialInteractionType.PlaceInFryer))
            return false;

        if (_fryingPastels.All(closed => closed != null))
            return false;

        if (_fryingPastels.All(closed => closed == null))
            StartFrying();
        
        var index = GetNearestSlotIndex(position);
        var newPosition = GetPositionForSlot(index);
        draggableClosedPastel.transform.position = newPosition;
        _fryingPastels[index] = draggableClosedPastel;
        _tutorialEvents.PublishPastelPlacedInFryer(draggableClosedPastel);
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
        
        _animator.SetBool("Frying", true);
    }

    private void StopFrying()
    {
        _fryingSource.Stop();
        foreach (var visualEffect in _visualEffects)
            visualEffect.Stop();
        
        _animator.SetBool("Frying", false);
    }

    private int GetNearestSlotIndex(Vector3 position)
    {
        return Array.IndexOf(_fryingPastels, null);
    }

    private Vector3 GetPositionForSlot(int index)
    {
        var size = _collider.size.x * 0.7f;

        var offset = new Vector3(size/4 * (index - 2) + size/8, _heightTransform.position.y, -1);
        return transform.position + offset;
    }

    private void Update()
    {
        foreach (var pastel in _fryingPastels.Where(pastel => pastel != null))
            pastel.Fry(Time.deltaTime);
    }
}
