using KBCore.Refs;
using PrimeTween;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;

public class OrderTakerBell : ValidatedMonoBehaviour, IPointerDownHandler
{
    [Inject]
    private readonly CustomerQueue _customerQueue;
    
    [Inject]
    private readonly OrderController _orderController;
    
    [Inject]
    private readonly ICustomerDialogue _customerDialogue;

    [Inject]
    private readonly GameplayInteractionGate _interactionGate;

    [Inject]
    private readonly TutorialTargetRegistry _tutorialTargetRegistry;

    [SerializeField]
    private AudioSource _bellSound;

    private TutorialTarget _tutorialTarget;

    private void Awake()
    {
        _tutorialTarget = GetComponent<TutorialTarget>() ?? gameObject.AddComponent<TutorialTarget>();
        _tutorialTarget.Configure(TutorialTargetId.OrderBell);
        _tutorialTargetRegistry.Register(_tutorialTarget);
    }

    private void OnDestroy()
    {
        _tutorialTargetRegistry.Unregister(_tutorialTarget);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_interactionGate.CanInteract(TutorialInteractionType.TakeOrder))
            return;

        if (_customerDialogue.IsPlaying || !_customerQueue.TryGetNext(out var customer))
            return;

        var order = _orderController.AcceptOrder(customer);

        _customerDialogue.OrderDialogue(order).ChainCallback(() => _orderController.StartOrder(order));
        _bellSound.Play();
    }
}
