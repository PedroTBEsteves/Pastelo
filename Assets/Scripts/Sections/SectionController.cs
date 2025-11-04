using System;
using Reflex.Attributes;
using UnityEngine;

public class SectionController : MonoBehaviour
{
    [SerializeField]
    private SceneSection _head;
    
    [SerializeField]
    private SceneSection _tail;

    [Inject]
    private readonly CameraController _cameraController;
    
    private float _xMax;
    private float _xMin;

    private void Awake()
    {
        _xMax = _tail.GetXMax();
        _xMin = _head.GetXMin();

        _cameraController.CameraMoved += OnCameraMoved;
    }

    private void OnCameraMoved()
    {
        var cameraViewRect = _cameraController.GetViewRect();

        while (cameraViewRect.xMax > _xMax)
        {
            _tail.MoveNext();
            _tail = _tail.Next;
            _xMax = _tail.GetXMax();
        }

        while (cameraViewRect.xMin < _xMin)
        {
            _head.MovePrevious();
            _head = _head.Previous;
            _xMin = _head.GetXMin();
        }
    }
}
