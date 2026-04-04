public class GameplayTutorialController
{
    private readonly GameplayTutorialState _state;
    private readonly GameplayTutorialEvents _tutorialEvents;
    private readonly TutorialTargetRegistry _targetRegistry;
    private readonly CustomerQueue _customerQueue;
    private readonly OrderController _orderController;
    private readonly CameraController _cameraController;

    private TutorialTarget _shownTarget;

    public GameplayTutorialController(
        GameplayTutorialState state,
        GameplayTutorialEvents tutorialEvents,
        TutorialTargetRegistry targetRegistry,
        CustomerQueue customerQueue,
        OrderController orderController,
        CameraController cameraController)
    {
        _state = state;
        _tutorialEvents = tutorialEvents;
        _targetRegistry = targetRegistry;
        _customerQueue = customerQueue;
        _orderController = orderController;
        _cameraController = cameraController;

        _state.StateChanged += RefreshTarget;
        _targetRegistry.TargetsChanged += RefreshTarget;

        _customerQueue.CustomerArrived += OnCustomerArrived;
        _orderController.OrderStarted += OnOrderStarted;
        _cameraController.CurrentSectionChanged += OnCameraSectionChanged;

        _tutorialEvents.DoughOpened += OnDoughOpened;
        _tutorialEvents.FillingAdded += OnFillingAdded;
        _tutorialEvents.PastelClosed += OnPastelClosed;
        _tutorialEvents.PastelPickedUp += OnPastelPickedUp;
        _tutorialEvents.PastelDropped += OnPastelDropped;
        _tutorialEvents.PastelPlacedInFryer += OnPastelPlacedInFryer;
        _tutorialEvents.PastelReachedCooked += OnPastelReachedCooked;
        _tutorialEvents.PastelRemovedFromFryer += OnPastelRemovedFromFryer;
        _tutorialEvents.PastelPlacedOnDelivery += OnPastelPlacedOnDelivery;
        _tutorialEvents.OrderDelivered += OnOrderDelivered;

        if (!GameplayTutorialOptions.ConsumeShouldRunTutorial())
            return;

        _state.Activate();
    }

    private void OnCustomerArrived(Customer _)
    {
        if (!_state.IsActive || _state.TutorialOrder != null || _state.CurrentStep != TutorialStep.WaitForCustomer)
            return;

        _state.SetStep(TutorialStep.TakeOrder, TutorialTargetId.OrderBell);
    }

    private void OnOrderStarted(Order order)
    {
        if (!_state.IsActive || _state.TutorialOrder != null)
            return;

        _state.BeginOrder(order);
        _state.SetStep(TutorialStep.MoveCameraToPrepping, TutorialTargetId.CameraMoveRight, expectedCameraSection: CameraSection.Prepping);
    }

    private void OnCameraSectionChanged(CameraSection section)
    {
        if (!_state.IsActive || _state.ExpectedCameraSection != section)
            return;

        switch (_state.CurrentStep)
        {
            case TutorialStep.MoveCameraToPrepping:
                _state.SetStep(TutorialStep.AddDough, TutorialTargetId.DoughSource, _state.ExpectedDough);
                break;
            case TutorialStep.MoveCameraToFrying:
                _state.SetStep(TutorialStep.PlaceInFrying, TutorialTargetId.FryingArea);
                break;
            case TutorialStep.MoveCameraToPacking:
                _state.SetStep(TutorialStep.PlaceOnDelivery, TutorialTargetId.DeliveryArea);
                break;
        }
    }

    private void OnDoughOpened(Dough dough)
    {
        if (!_state.IsActive || _state.CurrentStep != TutorialStep.AddDough || dough != _state.ExpectedDough)
            return;

        AdvanceAfterIngredient();
    }

    private void OnFillingAdded(Filling filling)
    {
        if (!_state.IsActive || _state.CurrentStep != TutorialStep.AddFilling || !_state.TryConsumeFilling(filling))
            return;

        AdvanceAfterIngredient();
    }

    private void AdvanceAfterIngredient()
    {
        if (_state.CurrentFilling != null)
        {
            _state.SetStep(TutorialStep.AddFilling, TutorialTargetId.FillingSource, _state.CurrentFilling);
            return;
        }

        _state.SetStep(TutorialStep.ClosePastel, TutorialTargetId.ClosePastelArea);
    }

    private void OnPastelClosed(DraggableClosedPastel pastel)
    {
        if (!_state.IsActive || _state.CurrentStep != TutorialStep.ClosePastel)
            return;

        _state.SetTutorialPastel(pastel);
        _state.SetStep(TutorialStep.ClosePastel, TutorialTargetId.CookedPastel, pastel);
    }

    private void OnPastelPickedUp(DraggableClosedPastel pastel)
    {
        if (!_state.IsActive)
            return;

        if (_state.CurrentStep == TutorialStep.ClosePastel && _state.TutorialPastel == pastel)
        {
            _state.TryBeginDraggingTutorialPastel(pastel);
            _state.SetStep(TutorialStep.MoveCameraToFrying, TutorialTargetId.CameraMoveRight, expectedCameraSection: CameraSection.Frying);
            return;
        }

        if (_state.CurrentStep == TutorialStep.RemoveCookedPastel && _state.TutorialPastel == pastel)
        {
            _state.TryBeginDraggingTutorialPastel(pastel);
            _state.SetStep(TutorialStep.MoveCameraToPacking, TutorialTargetId.CameraMoveRight, expectedCameraSection: CameraSection.Packing);
            return;
        }

        if (_state.CurrentStep is not (TutorialStep.MoveCameraToFrying or TutorialStep.MoveCameraToPacking))
            return;

        _state.TryBeginDraggingTutorialPastel(pastel);
    }

    private void OnPastelDropped(DraggableClosedPastel pastel)
    {
        if (!_state.IsActive || _state.CurrentStep is not (TutorialStep.MoveCameraToFrying or TutorialStep.MoveCameraToPacking))
            return;

        if (!_state.TryEndDraggingTutorialPastel(pastel))
            return;

        var returnStep = _state.CurrentStep == TutorialStep.MoveCameraToFrying
            ? TutorialStep.ClosePastel
            : TutorialStep.RemoveCookedPastel;
        _state.SetStep(returnStep, TutorialTargetId.CookedPastel, pastel);
    }

    private void OnPastelPlacedInFryer(DraggableClosedPastel _)
    {
        if (!_state.IsActive || _state.CurrentStep != TutorialStep.PlaceInFrying)
            return;

        _state.ClearTutorialPastelDrag();
        _state.SetStep(TutorialStep.WaitUntilCooked);
    }

    private void OnPastelReachedCooked(DraggableClosedPastel pastel)
    {
        if (!_state.IsActive || _state.CurrentStep != TutorialStep.WaitUntilCooked)
            return;

        _state.SetTutorialPastel(pastel);
        _state.SetStep(TutorialStep.RemoveCookedPastel, TutorialTargetId.CookedPastel, pastel);
    }

    private void OnPastelRemovedFromFryer(DraggableClosedPastel _)
    {
        // Advancing to packing is now driven by the drag state, not just by removing from the fryer.
    }

    private void OnPastelPlacedOnDelivery(DraggableClosedPastel _)
    {
        if (!_state.IsActive || _state.CurrentStep != TutorialStep.PlaceOnDelivery)
            return;

        _state.SetStep(TutorialStep.DeliverOrder, TutorialTargetId.OrderNote, _state.TutorialOrder);
    }

    private void OnOrderDelivered(Order order)
    {
        if (!_state.IsActive || order != _state.TutorialOrder || _state.CurrentStep != TutorialStep.DeliverOrder)
            return;

        _state.ClearTutorialPastelDrag();
        _state.Finish();
    }

    private void RefreshTarget()
    {
        _shownTarget?.Hide();
        _shownTarget = null;

        if (!_state.IsActive || _state.CurrentTargetId == TutorialTargetId.None)
            return;

        if (!_targetRegistry.TryGet(_state.CurrentTargetId, _state.CurrentTargetContext, out _shownTarget))
            return;

        _shownTarget.Show();
    }
}
