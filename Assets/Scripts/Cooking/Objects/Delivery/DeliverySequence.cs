public class DeliverySequence
{
    private readonly ICustomerDialogue _customerDialogue;
    private readonly TimeController _timeController;
    private readonly CameraController _cameraController;
    private readonly OrderController _orderController;
    private readonly StrikesController _strikesController;

    private bool _isFailedOrder;

    public DeliverySequence(ICustomerDialogue customerDialogue, TimeController timeController, CameraController cameraController, OrderController orderController, StrikesController strikesController)
    {
        _customerDialogue = customerDialogue;
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

        _customerDialogue.DeliveryDialogue(order, delivery, _orderController).ChainCallback(() =>
        {
            _timeController.Resume();

            if (_isFailedOrder)
            {
                _strikesController.Strike();
                _isFailedOrder = false;
            }
        });
    }

    private void Strike(Order _) => _isFailedOrder = true;
}
