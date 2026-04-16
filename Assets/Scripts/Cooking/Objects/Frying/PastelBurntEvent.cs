public readonly struct PastelBurntEvent : IEvent
{
    public PastelBurntEvent(DraggableClosedPastel pastel)
    {
        Pastel = pastel;
    }

    public DraggableClosedPastel Pastel { get; }
}
