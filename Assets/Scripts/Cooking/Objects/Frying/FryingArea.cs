using System;
using System.Linq;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.VFX;

public class FryingArea : ValidatedMonoBehaviour
{
    [Serializable]
    private struct FryingSlot
    {
        [SerializeField]
        private Transform _pastelTransform;

        [SerializeField]
        private int _sortingOrder;

        public Transform PastelTransform => _pastelTransform;
        public int SortingOrder => _sortingOrder;
    }
    
    [SerializeField, Child(Flag.ExcludeSelf)]
    private Animator _animator;
    
    [SerializeField]
    private Transform _discardPositionTransform;

    [SerializeField, Self]
    private AudioSource _fryingSource;

    [SerializeField, Child(Flag.ExcludeSelf)]
    private AudioSource _stoveSound;
    
    [SerializeField]
    private FryingSlot[] _slots;
    
    [SerializeField]
    private VisualEffect[] _visualEffects;

    [Inject]
    private readonly GameplayTutorialEvents _tutorialEvents;

    [Inject]
    private readonly GameplayInteractionGate _interactionGate;

    [Inject]
    private readonly TutorialTargetRegistry _tutorialTargetRegistry;
    
    private DraggableClosedPastel[] _fryingPastels;

    private TutorialTarget _tutorialTarget;
    
    public Vector3 DiscardPosition => _discardPositionTransform.position;

    private void Awake()
    {
        _slots ??= Array.Empty<FryingSlot>();
        _fryingPastels = new DraggableClosedPastel[_slots.Length];

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

        var index = GetNearestSlotIndex(position);
        if (index == -1)
            return false;

        if (_fryingPastels.All(closed => closed == null))
            StartFrying();
        
        var slot = _slots[index];
        var slotTransform = slot.PastelTransform;
        draggableClosedPastel.transform.SetParent(slotTransform, false);
        draggableClosedPastel.transform.localPosition = Vector3.zero;
        draggableClosedPastel.transform.localRotation = Quaternion.identity;
        draggableClosedPastel.SetSortingOrder(slot.SortingOrder);

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
            draggableClosedPastel.ReleaseFromFryingSlot();
            
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
        var nearestIndex = -1;
        var nearestDistance = float.MaxValue;

        for (var index = 0; index < _slots.Length; index++)
        {
            if (_fryingPastels[index] != null)
                continue;
            
            var slotTransform = _slots[index].PastelTransform;

            
            var distance = Mathf.Abs(slotTransform.position.y - position.y);
            if (distance >= nearestDistance)
                continue;
            
            nearestIndex = index;
            nearestDistance = distance;
        }
        
        return nearestIndex;
    }

    private void Update()
    {
        foreach (var pastel in _fryingPastels.Where(pastel => pastel != null))
            pastel.Fry(Time.deltaTime);
    }
}
