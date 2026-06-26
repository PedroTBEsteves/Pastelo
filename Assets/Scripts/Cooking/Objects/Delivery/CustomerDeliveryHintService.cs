public sealed class CustomerDeliveryHintService
{
    private CustomerDisplay _customerDisplay;

    public void Register(CustomerDisplay customerDisplay)
    {
        _customerDisplay = customerDisplay;
    }

    public void Unregister(CustomerDisplay customerDisplay)
    {
        if (_customerDisplay == customerDisplay)
            _customerDisplay = null;
    }

    public void SetHintsVisible(bool visible)
    {
        _customerDisplay?.SetHintsVisible(visible);
    }
}
