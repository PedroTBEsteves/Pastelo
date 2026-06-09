using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;

public class DraggableModeToggle : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private Toggle _toggle;

    private void Awake()
    {
        if (_toggle == null)
            return;

        _toggle.isOn = true;
        _toggle.interactable = false;
    }
}
