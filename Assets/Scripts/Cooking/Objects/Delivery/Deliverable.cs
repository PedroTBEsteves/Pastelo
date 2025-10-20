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
    
    private Pastel _pastel;
    
    public Vector3 DiscardPosition => _discardPositionTransform.position;
    
    public bool TryAddPastel(DraggableClosedPastel closedPastel)
    {
        if (_pastel != null)
            return false;

        _pastel = closedPastel.GetPastel();
        _addedAudioSource.Play();
        return true;
    }

    public bool TryDeliver(OrderNote orderNote)
    {
        if (_pastel == null)
            return false;

        var delivery = new Delivery(_pastel);
        _addedAudioSource.Play();
        _deliverySequence.StartSequence(orderNote.Order, delivery);

        _pastel = null;
        
        return true;
    }
}
