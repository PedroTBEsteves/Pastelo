using System;
using KBCore.Refs;
using Reflex.Attributes;
using TMPro;
using UnityEngine;

public class QueueLength : MonoBehaviour
{
    [SerializeField, Self]
    private TextMeshProUGUI _text;
    
    [Inject]
    private readonly CustomerQueue _customerQueue;

    private void Awake()
    {
        _customerQueue.CustomersCountChanged += OnCustomersCountChanged;
    }

    private void OnCustomersCountChanged(int count)
    {
        _text.SetText(count.ToString());
    }
}
