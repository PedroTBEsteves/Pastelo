using Reflex.Attributes;
using UnityEngine;

public class Deliverable : MonoBehaviour
{
    [SerializeField] 
    private Transform _discardPositionTransform;
    
    [SerializeField]
    private AudioSource _addedAudioSource;
    
    [Inject]
    private readonly DeliverySequence _deliverySequence;

    [Inject]
    private readonly GameplayTutorialEvents _tutorialEvents;

    [Inject]
    private readonly GameplayInteractionGate _interactionGate;

    [Inject]
    private readonly TutorialTargetRegistry _tutorialTargetRegistry;
    
    private Pastel _pastel;
    private TutorialTarget _tutorialTarget;
    
    public Vector3 DiscardPosition => _discardPositionTransform.position;

    private void Awake()
    {
        _tutorialTarget = GetComponent<TutorialTarget>() ?? gameObject.AddComponent<TutorialTarget>();
        _tutorialTarget.Configure(TutorialTargetId.DeliveryArea);
        _tutorialTargetRegistry.Register(_tutorialTarget);
    }

    private void OnDestroy()
    {
        _tutorialTargetRegistry.Unregister(_tutorialTarget);
    }
    
    public bool TryAddPastel(DraggableClosedPastel closedPastel)
    {
        if (!_interactionGate.CanInteract(TutorialInteractionType.PlaceOnDelivery))
            return false;

        if (_pastel != null)
            return false;

        _pastel = closedPastel.GetPastel();
        _addedAudioSource.Play();
        _tutorialEvents.PublishPastelPlacedOnDelivery(closedPastel);
        return true;
    }

    public bool TryDeliver(OrderNote orderNote)
    {
        if (!_interactionGate.CanInteract(TutorialInteractionType.DeliverOrder, orderNote.Order))
            return false;

        if (_pastel == null)
            return false;

        var delivery = new Delivery(_pastel);
        _addedAudioSource.Play();
        _deliverySequence.StartSequence(orderNote.Order, delivery);
        _tutorialEvents.PublishOrderDelivered(orderNote.Order);

        _pastel = null;
        
        return true;
    }
}
