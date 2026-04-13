using System;
using UnityEngine;

public sealed class DraggableInputConfiguration
{
    private const string PlayerPrefsKey = "draggable_input_mode";

    private readonly IDraggableHandler _dragHandler = new DragDraggableHandler();
    private readonly IDraggableHandler _clickHandler = new ClickDraggableHandler();

    public DraggableInputMode Mode
    {
        get
        {
            var savedValue = PlayerPrefs.GetInt(PlayerPrefsKey, (int)DraggableInputMode.Drag);
            return Enum.IsDefined(typeof(DraggableInputMode), savedValue)
                ? (DraggableInputMode)savedValue
                : DraggableInputMode.Drag;
        }
        set
        {
            var sanitizedMode = value == DraggableInputMode.Click
                ? DraggableInputMode.Click
                : DraggableInputMode.Drag;

            PlayerPrefs.SetInt(PlayerPrefsKey, (int)sanitizedMode);
            PlayerPrefs.Save();
        }
    }

    public IDraggableHandler CurrentHandler => Mode == DraggableInputMode.Click
        ? _clickHandler
        : _dragHandler;
}
