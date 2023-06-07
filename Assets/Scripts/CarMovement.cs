using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class CarMovement : MonoBehaviour
{
    protected Element _currentNode; // The current node the car is at
    protected Vector3 _moveDir;
    protected readonly float _speed = 4f;
    protected Collider2D coll;

    protected ObjectPool<GameObject> _pool;
    public void SetPool(ObjectPool<GameObject> pool) => _pool = pool;

    private void Awake()
    {
        coll = GetComponent<Collider2D>();
    }

    protected virtual Vector3 GetPositionFromNode(Element node)
    {
        // Convert the node's latitude and longitude to a Vector3 position
        return new Vector3(node.lon * 1000, node.lat * 1000, 0f);
    }

    protected virtual void RotateCarTowardsNode(Element node)
    {
        Vector3 carPosition = transform.position;
        Vector3 nodePosition = GetPositionFromNode(node);
        Vector3 direction = (nodePosition - carPosition).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float newAngle = angle + 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
    }
}
