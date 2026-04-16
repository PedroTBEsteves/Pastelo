public class DeliverySequence
{
    private readonly ICustomerDialogue _customerDialogue;
    private readonly TimeController _timeController;
    private readonly CameraController _cameraController;
    private readonly OrderController _orderController;

    public DeliverySequence(ICustomerDialogue customerDialogue, TimeController timeController, CameraController cameraController, OrderController orderController)
    {
        _customerDialogue = customerDialogue;
        _timeController = timeController;
        _cameraController = cameraController;
        _orderController = orderController;
    }
    
    public void StartSequence(Order order, Delivery delivery)
    {
        _timeController.Pause();
        _cameraController.GoImmediatelyToSection(CameraSection.Balcony);

        _customerDialogue.DeliveryDialogue(order, delivery, _orderController).ChainCallback(() =>
        {
            _timeController.Resume();
            _orderController.FinishOrderFlow(order);
        });
    }
}
