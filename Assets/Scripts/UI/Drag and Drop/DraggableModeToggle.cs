using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

public class DraggableModeToggle : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private Toggle _toggle;
    
    [Inject]
    private readonly DraggableInputConfiguration _inputConfiguration;

    private void Awake()
    {
        _toggle.isOn = _inputConfiguration.Mode != DraggableInputMode.Drag;
        _toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    private void OnToggleValueChanged(bool isClick)
    {
        var mode = isClick ? DraggableInputMode.Click :  DraggableInputMode.Drag;
        _inputConfiguration.Mode = mode;
    }
}
