using UnityEngine;

public class OrderNoteFillingGroup : MonoBehaviour
{
    [SerializeField]
    private OrderNoteFilling _fillingPrefab;

    [SerializeField]
    private int _maxFillings;

    private int _fillingsCount;

    public bool TryAdd(Filling filling, int amount)
    {
        if (_fillingsCount >= _maxFillings)
            return false;
        
        var noteFilling = Instantiate(_fillingPrefab, transform);
        noteFilling.Initialize(filling, amount);
        _fillingsCount++;
        return true;
    }
}
