using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LoadoutSettings", menuName = "Scriptable Objects/LoadoutSettings")]
public sealed class LoadoutSettings : ScriptableObject
{
    [SerializeField]
    private LoadoutUpgradeLevel[] _doughLevels = Array.Empty<LoadoutUpgradeLevel>();

    [SerializeField]
    private LoadoutUpgradeLevel[] _fillingLevels = Array.Empty<LoadoutUpgradeLevel>();

    public IReadOnlyList<LoadoutUpgradeLevel> DoughLevels => _doughLevels;
    public IReadOnlyList<LoadoutUpgradeLevel> FillingLevels => _fillingLevels;
}

[Serializable]
public struct LoadoutUpgradeLevel
{
    [SerializeField, Min(0)]
    private int _maxAmount;

    [SerializeField, Min(0)]
    private float _purchasePrice;

    [SerializeField]
    private GameObject _areaPrefab;

    public int MaxAmount => Mathf.Max(0, _maxAmount);
    public float PurchasePrice => Mathf.Max(0, _purchasePrice);
    public GameObject AreaPrefab => _areaPrefab;
}
