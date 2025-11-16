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

    public void AttachToSection(Transform objectToAttach)
    {
        var position = objectToAttach.position.x;
        var head = _head;
        while (!head.IsInside(position))
            head = head.Next;

        objectToAttach.transform.parent = head.transform;
    }
    
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
            
            _head = _head.Next;
            _xMin = _head.GetXMin();
        }

        while (cameraViewRect.xMin < _xMin)
        {
            _head.MovePrevious();
            _head = _head.Previous;
            _xMin = _head.GetXMin();
            
            _tail = _tail.Previous;
            _xMax = _tail.GetXMax();
        }
    }
}
