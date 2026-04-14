using AYellowpaper.SerializedCollections;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Dough", menuName = "Scriptable Objects/Dough")]
public class Dough : Ingredient
{
    [field: SerializeField]
    public Sprite OpenDoughSprite { get; private set; }

    [SerializeField]
    private Sprite[] _closeDragBaseLayerFrames = Array.Empty<Sprite>();

    [SerializeField]
    private Sprite[] _closeDragCoverLayerFrames = Array.Empty<Sprite>();
    
    [SerializeField]
    private SerializedDictionary<FriedLevel, Sprite> _closedPastelDoughSprites;

    public Sprite GetClosedDoughSprite(FriedLevel level) => _closedPastelDoughSprites[level];

    public int GetCloseDragFrameCount() => Mathf.Min(_closeDragBaseLayerFrames?.Length ?? 0, _closeDragCoverLayerFrames?.Length ?? 0);

    public Sprite GetCloseDragBaseLayerFrame(int index) => GetFrame(_closeDragBaseLayerFrames, index);

    public Sprite GetCloseDragCoverLayerFrame(int index) => GetFrame(_closeDragCoverLayerFrames, index);

    private static Sprite GetFrame(Sprite[] frames, int index)
    {
        if (frames == null || index < 0 || index >= frames.Length)
            return null;

        return frames[index];
    }
}
