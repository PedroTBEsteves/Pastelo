using System;
using System.Collections.Generic;
using System.Linq;

public class GameplayTutorialState
{
    private readonly Queue<Filling> _remainingFillings = new();

    public GameplayTutorialState(Recipe tutorialRecipe)
    {
        TutorialRecipe = tutorialRecipe;
    }

    public bool IsActive { get; private set; }
    public bool IsFinished { get; private set; }
    public TutorialStep CurrentStep { get; private set; } = TutorialStep.Inactive;
    public TutorialTargetId CurrentTargetId { get; private set; } = TutorialTargetId.None;
    public object CurrentTargetContext { get; private set; }
    public CameraSection? ExpectedCameraSection { get; private set; }
    public Order TutorialOrder { get; private set; }
    public DraggableClosedPastel TutorialPastel { get; private set; }
    public bool IsTutorialPastelBeingDragged { get; private set; }
    public Dough ExpectedDough => TutorialOrder?.Recipe.Dough;
    public Filling CurrentFilling => _remainingFillings.Count > 0 ? _remainingFillings.Peek() : null;
    public Recipe TutorialRecipe { get; }

    public event Action StateChanged = delegate { };

    public void Activate()
    {
        IsActive = true;
        IsFinished = false;
        SetStep(TutorialStep.WaitForCustomer);
    }

    public void BeginOrder(Order order)
    {
        TutorialOrder = order;
        _remainingFillings.Clear();

        foreach (var filling in order.Recipe.Fillings
                     .OrderBy(pair => pair.Key.Name)
                     .SelectMany(pair => Enumerable.Repeat(pair.Key, pair.Value)))
        {
            _remainingFillings.Enqueue(filling);
        }

        StateChanged();
    }

    public bool TryConsumeFilling(Filling filling)
    {
        if (_remainingFillings.Count == 0 || _remainingFillings.Peek() != filling)
            return false;

        _remainingFillings.Dequeue();
        StateChanged();
        return true;
    }

    public void SetStep(
        TutorialStep step,
        TutorialTargetId targetId = TutorialTargetId.None,
        object targetContext = null,
        CameraSection? expectedCameraSection = null)
    {
        CurrentStep = step;
        CurrentTargetId = targetId;
        CurrentTargetContext = targetContext;
        ExpectedCameraSection = expectedCameraSection;
        StateChanged();
    }

    public void Finish()
    {
        IsFinished = true;
        IsActive = false;
        ClearTutorialPastelDrag();
        SetStep(TutorialStep.Finished);
    }

    public void SetTutorialPastel(DraggableClosedPastel pastel)
    {
        TutorialPastel = pastel;
        IsTutorialPastelBeingDragged = false;
        StateChanged();
    }

    public bool TryBeginDraggingTutorialPastel(DraggableClosedPastel pastel)
    {
        if (TutorialPastel != pastel)
            return false;

        if (IsTutorialPastelBeingDragged)
            return true;

        IsTutorialPastelBeingDragged = true;
        StateChanged();
        return true;
    }

    public bool TryEndDraggingTutorialPastel(DraggableClosedPastel pastel)
    {
        if (TutorialPastel != pastel)
            return false;

        if (!IsTutorialPastelBeingDragged)
            return true;

        IsTutorialPastelBeingDragged = false;
        StateChanged();
        return true;
    }

    public void ClearTutorialPastelDrag()
    {
        if (TutorialPastel == null && !IsTutorialPastelBeingDragged)
            return;

        TutorialPastel = null;
        IsTutorialPastelBeingDragged = false;
        StateChanged();
    }
}
