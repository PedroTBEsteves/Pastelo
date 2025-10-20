using Reflex.Attributes;

public class DeliverySequence
{
    [Inject]
    private ICustomerDialogue _customerDialogue;
    
    [Inject]
    private readonly TimeController _timeController;
    
    [Inject]
    private readonly CameraController _cameraController;
    
    [Inject]
    private readonly OrderController _orderController;
    
    public void StartSequence(Order order, Delivery delivery)
    {
        _timeController.Pause();
        _cameraController.GoImmediatelyToSection(CameraSection.Balcony);

        _customerDialogue.DeliveryDialogue(order, delivery, _orderController).ChainCallback(() =>
        {
            _timeController.Resume();
        });
    }
}
