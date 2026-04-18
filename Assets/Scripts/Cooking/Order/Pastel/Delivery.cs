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
        if (order?.Recipe == null)
            return false;

        var pastelIsCorrect = Pastel.IsCorrectFor(order.Recipe);

        return pastelIsCorrect;
    }
}
