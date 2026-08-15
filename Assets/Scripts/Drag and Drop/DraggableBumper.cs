using KBCore.Refs;
using UnityEngine;

public class DraggableBumper : ValidatedMonoBehaviour
{
    [SerializeField, Self]
    private BoxCollider2D _boxCollider2D;
    
    public void Bump(Transform target)
    {
        var localPosition = transform.InverseTransformPoint(target.position);

        var center = _boxCollider2D.offset;
        var halfSize = _boxCollider2D.size * 0.5f;
        
        var x = center.x + halfSize.x;
        var y = Mathf.Clamp(localPosition.y, center.y - halfSize.y, center.y + halfSize.y);
        
        var bumpedPosition = transform.TransformPoint(new Vector3(x, y, localPosition.z));
        var movement = bumpedPosition - target.position;
        
        if (Vector2.Dot(movement, transform.right) > 0f)
            target.transform.position = bumpedPosition;
    }
}
