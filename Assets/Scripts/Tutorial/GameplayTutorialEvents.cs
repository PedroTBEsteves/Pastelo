using System;

public class GameplayTutorialEvents
{
    public event Action<Dough> DoughOpened = delegate { };
    public event Action<Filling> FillingAdded = delegate { };
    public event Action PastelClosed = delegate { };
    public event Action<DraggableClosedPastel> PastelPlacedInFryer = delegate { };
    public event Action<DraggableClosedPastel> PastelReachedCooked = delegate { };
    public event Action<DraggableClosedPastel> PastelRemovedFromFryer = delegate { };
    public event Action<DraggableClosedPastel> PastelPlacedOnDelivery = delegate { };
    public event Action<Order> OrderDelivered = delegate { };

    public void PublishDoughOpened(Dough dough) => DoughOpened(dough);
    public void PublishFillingAdded(Filling filling) => FillingAdded(filling);
    public void PublishPastelClosed() => PastelClosed();
    public void PublishPastelPlacedInFryer(DraggableClosedPastel pastel) => PastelPlacedInFryer(pastel);
    public void PublishPastelReachedCooked(DraggableClosedPastel pastel) => PastelReachedCooked(pastel);
    public void PublishPastelRemovedFromFryer(DraggableClosedPastel pastel) => PastelRemovedFromFryer(pastel);
    public void PublishPastelPlacedOnDelivery(DraggableClosedPastel pastel) => PastelPlacedOnDelivery(pastel);
    public void PublishOrderDelivered(Order order) => OrderDelivered(order);
}
