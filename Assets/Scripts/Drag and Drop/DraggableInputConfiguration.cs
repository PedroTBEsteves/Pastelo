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
            var savedValue = PlayerPrefs.GetInt(PlayerPrefsKey, (int)DraggableInputMode.Click);
            return Enum.IsDefined(typeof(DraggableInputMode), savedValue)
                ? (DraggableInputMode)savedValue
                : DraggableInputMode.Click;
        }
        set
        {
            var sanitizedMode = value == DraggableInputMode.Drag
                ? DraggableInputMode.Drag
                : DraggableInputMode.Click;

            PlayerPrefs.SetInt(PlayerPrefsKey, (int)sanitizedMode);
            PlayerPrefs.Save();
        }
    }

    public IDraggableHandler CurrentHandler => Mode == DraggableInputMode.Click
        ? _clickHandler
        : _dragHandler;
}
