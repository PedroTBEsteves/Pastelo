using Reflex.Attributes;
using UnityEngine;

public class MouseParallaxLayer : MonoBehaviour
{
    [SerializeField]
    private Vector2 _intensity = Vector2.one;

    [Inject]
    private readonly MouseParallaxService _mouseParallaxService;

    private Vector3 _appliedOffset;

    private void LateUpdate()
    {
        if (_mouseParallaxService == null)
            return;

        var truePosition = transform.position - _appliedOffset;
        var parallaxOffset = Vector2.Scale(_mouseParallaxService.CurrentOffset, _intensity);
        _appliedOffset = new Vector3(parallaxOffset.x, parallaxOffset.y, 0f);
        transform.position = truePosition + _appliedOffset;
    }

    private void OnDisable()
    {
        ResetOffset();
    }

    private void OnDestroy()
    {
        ResetOffset();
    }

    private void ResetOffset()
    {
        if (_appliedOffset == Vector3.zero)
            return;

        transform.position -= _appliedOffset;
        _appliedOffset = Vector3.zero;
    }
}
