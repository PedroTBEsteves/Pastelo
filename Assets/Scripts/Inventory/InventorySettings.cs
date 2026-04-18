using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "InventorySettings", menuName = "Scriptable Objects/InventorySettings")]
public class InventorySettings : ScriptableObject
{
    [SerializeField]
    private SerializedDictionary<ItemDefinition, int> _initialItems;
    
    public IEnumerable<(ItemDefinition Item, int Amount)> InitialItems => _initialItems.Select(kv => (kv.Key, kv.Value));
}
