using UnityEngine;

public class SectionController : MonoBehaviour
{
    [SerializeField]
    private SceneSection _head;
    
    [SerializeField]
    private SceneSection _tail;

    private float _xMax;
    private float _xMin;

    public void AttachToSection(Transform objectToAttach)
    {
        var position = objectToAttach.position.x;
        var head = GetSectionAt(position);

        objectToAttach.transform.parent = head.transform;
    }

    public Vector3 GetNextSectionPosition(float currentCameraX)
    {
        var current = GetSectionAt(currentCameraX);
        if (current == _tail)
            MoveRight();

        return current.Next.transform.position;
    }

    public Vector3 GetPreviousSectionPosition(float currentCameraX)
    {
        var current = GetSectionAt(currentCameraX);
        if (current == _head)
            MoveLeft();

        return current.Previous.transform.position;
    }
    
    private void Awake()
    {
        _xMax = _tail.GetXMax();
        _xMin = _head.GetXMin();
    }

    public void SyncToViewRect(Rect cameraViewRect)
    {
        while (cameraViewRect.xMax > _xMax)
            MoveRight();

        while (cameraViewRect.xMin < _xMin)
            MoveLeft();
    }

    private SceneSection GetSectionAt(float position)
    {
        var section = _head;
        do
        {
            if (section.IsInside(position))
                return section;

            section = section.Next;
        } while (section != _head);

        return GetClosestSection(position);
    }

    private SceneSection GetClosestSection(float position)
    {
        var section = _head;
        var closest = _head;
        var closestDistance = float.MaxValue;

        do
        {
            var distance = Mathf.Abs(section.transform.position.x - position);
            if (distance < closestDistance)
            {
                closest = section;
                closestDistance = distance;
            }

            section = section.Next;
        } while (section != _head);

        return closest;
    }

    private void MoveRight()
    {
        _tail.MoveNext();
        _tail = _tail.Next;
        _xMax = _tail.GetXMax();
        
        _head = _head.Next;
        _xMin = _head.GetXMin();
    }

    private void MoveLeft()
    {
        _head.MovePrevious();
        _head = _head.Previous;
        _xMin = _head.GetXMin();
        
        _tail = _tail.Previous;
        _xMax = _tail.GetXMax();
    }
}
