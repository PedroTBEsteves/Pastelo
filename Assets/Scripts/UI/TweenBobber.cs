using System;
using PrimeTween;
using UnityEngine;

public class TweenBobber : MonoBehaviour
{
    [SerializeField]
    private Direction _direction = Direction.Vertical;
    
    [SerializeField]
    private float _bobbingRange = 0.1f;

    [SerializeField]
    private TweenSettings _bobbingTweenSettings = new(1f, Ease.InOutSine, cycles: -1, cycleMode: CycleMode.Yoyo);
    
    void Start()
    {
        var position = transform.localPosition;

        switch (_direction)
        {
            case Direction.Vertical:
                Tween.LocalPositionY(
                    transform,
                    position.y,
                    position.y + _bobbingRange,
                    _bobbingTweenSettings);
                break;
            case Direction.Horizontal:
                Tween.LocalPositionX(
                    transform,
                    position.x,
                    position.x + _bobbingRange,
                    _bobbingTweenSettings);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    [Serializable]
    private enum Direction
    {
        Vertical,
        Horizontal,
    }
}
