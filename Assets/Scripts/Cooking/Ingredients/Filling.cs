using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Filling", menuName = "Scriptable Objects/Filling")]
public class Filling : Ingredient
{
    public override string GetName() => Name.GetLocalizedString(new { amount = 1 });
    
    [SerializeField]
    private FillingTag[] _tags;
    
    public IReadOnlyList<FillingTag> Tags => _tags;
    
    public bool HasTag(FillingTag tag) => _tags.Contains(tag);
}
