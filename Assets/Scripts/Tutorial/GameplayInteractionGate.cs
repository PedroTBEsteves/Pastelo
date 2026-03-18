public class GameplayInteractionGate
{
    private readonly GameplayTutorialState _state;

    public GameplayInteractionGate(GameplayTutorialState state)
    {
        _state = state;
    }

    public bool CanInteract(TutorialInteractionType interactionType, object context = null)
    {
        if (!_state.IsActive || _state.IsFinished)
            return true;

        return interactionType switch
        {
            TutorialInteractionType.TakeOrder => _state.CurrentStep == TutorialStep.TakeOrder,
            TutorialInteractionType.MoveCamera => _state.CurrentStep is TutorialStep.MoveCameraToPrepping
                or TutorialStep.MoveCameraToFrying
                or TutorialStep.MoveCameraToPacking
                or TutorialStep.PlaceInFrying
                or TutorialStep.PlaceOnDelivery,
            TutorialInteractionType.UseDough => _state.CurrentStep == TutorialStep.AddDough
                && Equals(_state.ExpectedDough, context),
            TutorialInteractionType.AddFilling => _state.CurrentStep == TutorialStep.AddFilling
                && Equals(_state.CurrentFilling, context),
            TutorialInteractionType.ClosePastel => _state.CurrentStep == TutorialStep.ClosePastel,
            TutorialInteractionType.PlaceInFryer => _state.CurrentStep == TutorialStep.PlaceInFrying,
            TutorialInteractionType.RemoveCookedPastel => _state.CurrentStep is TutorialStep.RemoveCookedPastel
                or TutorialStep.MoveCameraToPacking
                or TutorialStep.PlaceOnDelivery,
            TutorialInteractionType.PlaceOnDelivery => _state.CurrentStep == TutorialStep.PlaceOnDelivery,
            TutorialInteractionType.DeliverOrder => _state.CurrentStep == TutorialStep.DeliverOrder
                && (_state.TutorialOrder == null || Equals(_state.TutorialOrder, context)),
            TutorialInteractionType.DiscardItem => false,
            TutorialInteractionType.BuyIngredient => false,
            _ => true,
        };
    }
}
