using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "Scriptable Objects/Level", fileName = "Level")]
public class Level : ScriptableObject
{
    [field: SerializeField]
    public LocalizedString Name { get; private set; }

    [field: SerializeField]
    public LocalizedString Description { get; private set; }

    [SerializeField]
    private Customer[] _customers;
    
    [field: Min(0f)]
    [field: SerializeField]
    public float LevelDurationSeconds { get; private set; }
    
    [field: SerializeField]
    public float PriceToPlay { get; private set; }
    
    [field: SerializeField]
    public Sprite SplashImage { get; private set; }

    [SerializeField]
    private SerializedDictionary<CameraSection, Sprite> _sectionBackgroundSprites;
    
    [SerializeField]
    private Dough[] _preferredDoughs;
    
    [SerializeField]
    private Filling[] _preferredFillings;
    
    [SerializeField]
    private FillingTag[] _preferredFillingTags;

    public IEnumerable<Customer> Customers => _customers;
    
    public IReadOnlyList<Dough> PreferredDoughs => _preferredDoughs;
    
    public IReadOnlyList<Filling> PreferredFillings => _preferredFillings;
    
    public IReadOnlyList<FillingTag> PreferredFillingTags => _preferredFillingTags;

    public Sprite GetSectionBackgroundSprite(CameraSection section)
    {
        if (_sectionBackgroundSprites != null &&
            _sectionBackgroundSprites.TryGetValue(section, out var sprite) &&
            sprite != null)
        {
            return sprite;
        }

        throw new InvalidOperationException(
            $"{nameof(Level)} '{name}' requires a background sprite configured for {nameof(CameraSection)}.{section}.");
    }
}
