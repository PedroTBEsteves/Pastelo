public class DeliverySequence
{
    private readonly ICustomerDeliveryDialogue _customerDeliveryDialogue;
    private readonly TimeController _timeController;
    private readonly CameraController _cameraController;
    private readonly OrderController _orderController;
    private readonly StrikesController _strikesController;

    private bool _isFailedOrder;

    public DeliverySequence(ICustomerDeliveryDialogue customerDeliveryDialogue, TimeController timeController, CameraController cameraController, OrderController orderController, StrikesController strikesController)
    {
        _customerDeliveryDialogue = customerDeliveryDialogue;
        _timeController = timeController;
        _cameraController = cameraController;
        _orderController = orderController;
        _strikesController = strikesController;

        _orderController.OrderFailed += Strike;
    }
    
    public void StartSequence(Order order, Delivery delivery)
    {
        _timeController.Pause();
        _cameraController.GoImmediatelyToSection(CameraSection.Balcony);

        _customerDeliveryDialogue.DeliveryDialogue(order, delivery, _orderController).ChainCallback(() =>
        {
            _timeController.Resume();

            if (_isFailedOrder)
            {
                _strikesController.Strike();
                _isFailedOrder = false;
            }

            _orderController.FinishOrderFlow(order);
        });
    }

    private void Strike(Order _) => _isFailedOrder = true;
}
