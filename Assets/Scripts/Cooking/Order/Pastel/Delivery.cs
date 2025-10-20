using UnityEngine;

public class Delivery
{
    public Delivery(Pastel pastel)
    {
        Pastel = pastel;
    }

    public Pastel Pastel { get; }

    public bool IsCorrectFor(Order order)
    {
        var pastelIsCorrect = Pastel.IsCorrectFor(order.Recipe);

        return pastelIsCorrect;
    }
}
