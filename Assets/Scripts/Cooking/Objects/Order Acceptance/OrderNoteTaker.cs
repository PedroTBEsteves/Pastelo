using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

public class OrderNoteTaker : MonoBehaviour
{
    [SerializeField]
    private OrderNote _orderNotePrefab;
    
    [Inject]
    private OrderController _orderController;

    private void Awake()
    {
        _orderController.OrderStarted += OnOrderStarted;
    }

    private void OnOrderStarted(Order order)
    {
        var orderNote = Instantiate(_orderNotePrefab, transform);
        orderNote.Initialize(order);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        orderNote.PostInitialize();
    }
}
