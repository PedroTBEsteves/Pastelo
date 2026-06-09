using System.Collections.Generic;
using Reflex.Attributes;
using UnityEngine;

public class Ticker : MonoBehaviour
{
    [Inject]
    private IEnumerable<ITickable> _tickables;

    [Inject]
    private TimeController _timeController;

    [Inject]
    private GameplayTutorialController _gameplayTutorialController;

    [Inject]
    private LevelPerformanceTracker _levelPerformanceTracker;

    [Inject]
    private ArcadeIngredientProgression _arcadeIngredientProgression;

    [Inject]
    private ArcadeFailureStrikeController _arcadeFailureStrikeController;
    
    private void Update()
    {
        if (!_timeController.Running)
            return;
        
        foreach (var tickable in _tickables)
            tickable.Tick(Time.deltaTime);
    }
}
