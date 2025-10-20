using System.Collections.Generic;
using Reflex.Attributes;
using UnityEngine;

public class Ticker : MonoBehaviour
{
    [Inject]
    private IEnumerable<ITickable> _tickables;

    [Inject]
    private TimeController _timeController;
    
    private void Update()
    {
        if (!_timeController.Running)
            return;
        
        foreach (var tickable in _tickables)
            tickable.Tick(Time.deltaTime);
    }
}
