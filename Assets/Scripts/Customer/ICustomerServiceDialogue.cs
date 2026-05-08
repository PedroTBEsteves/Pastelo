using PrimeTween;

public interface ICustomerServiceDialogue
{
    Sequence OrderDialogue(Order order);

    bool IsPlaying { get; }
}
