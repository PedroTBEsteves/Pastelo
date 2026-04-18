using System;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;

public class ToggleGameObjectButton : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private Button _button;
    
    [SerializeField]
    private GameObject _gameObject;

    private void Awake()
    {
        _button.onClick.AddListener(ToggleGameObject);
    }

    private void ToggleGameObject()
    {
        _gameObject.SetActive(!_gameObject.activeSelf);
    }
}
