using PrimeTween;
using UnityEngine;

public class TweenBobber : MonoBehaviour
{
    [SerializeField]
    private float _bobbingRange = 0.1f;

    [SerializeField]
    private TweenSettings _bobbingTweenSettings = new(1f, Ease.InOutSine, cycles: -1, cycleMode: CycleMode.Yoyo);
    
    void Start()
    {
        var position = transform.localPosition;
        
        Tween.LocalPositionY(
            transform,
            position.y,
            position.y + _bobbingRange,
            _bobbingTweenSettings);
    }
}
