using PrimeTween;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LevelTimeIndicator : MonoBehaviour
{
    [Inject]
    private readonly LevelFlowController _levelFlowController;

    [SerializeField]
    private Slider _slider;

    [SerializeField]
    private Image _knobImage;

    [SerializeField]
    private Image _moonImage;

    [SerializeField]
    private TweenSettings _knobChangeTweenSettings;
    
    private void Awake()
    {
        _slider.normalizedValue = 1f;
        var color = _moonImage.color;
        color.a = 0f;
        _moonImage.color = color;
        _levelFlowController.LevelTimeChanged += OnLevelTimeChanged;
    }

    private void OnDestroy()
    {
        if (_levelFlowController != null)
            _levelFlowController.LevelTimeChanged -= OnLevelTimeChanged;
    }

    private void OnLevelTimeChanged(float normalizedElapsedTime)
    {
        var normalizedRemainingTime = Mathf.Clamp01(1f - normalizedElapsedTime);
        _slider.normalizedValue = normalizedRemainingTime;

        if (normalizedRemainingTime > 0f)
            return;

        var knobAnimation = Sequence.Create()
            .Group(Tween.Alpha(_knobImage, 0f, _knobChangeTweenSettings))
            .Group(Tween.EulerAngles(_knobImage.transform, Vector3.zero, new Vector3(0f, 0f, 360f),
                _knobChangeTweenSettings));

        var moonAnimation = Sequence.Create()
            .Group(Tween.Alpha(_moonImage, 1f, _knobChangeTweenSettings))
            .Group(Tween.EulerAngles(_moonImage.transform, Vector3.zero, new Vector3(0f, 0f, 360f),
                _knobChangeTweenSettings));

        Sequence.Create()
            .Group(knobAnimation)
            .Group(moonAnimation);
    }
}
