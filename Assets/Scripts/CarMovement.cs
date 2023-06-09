using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class CarMovement : MonoBehaviour
{
    protected Element _currentNode; // The current node the car is at
    protected Vector3 _moveDir;
    protected readonly float _speed = 10f;
    protected Collider2D coll;

    protected ObjectPool<GameObject> _pool;
    public void SetPool(ObjectPool<GameObject> pool) => _pool = pool;

    private void Awake()
    {
        coll = GetComponent<Collider2D>();
    }
    public virtual void StartRoute()
    {
        transform.position = GetPositionFromNode(_currentNode);
    }

    protected virtual Vector3 GetPositionFromNode(Element node)
    {
        // Convert the node's latitude and longitude to a Vector3 position
        float scale = 10000;
        return new Vector3(node.lon * scale - 133350f, node.lat * scale - 525100f, 0f);
    }

    protected Element FindNodeFromID(long nodeId)
    {
        JSONData mapData = MapView.Instance.MapData;
        Element node = mapData.elements.Find(element => element.id == nodeId);
        return node;
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

    protected virtual bool DetectObstacleAhead()
    {
        Vector3 carPosition = transform.position;
        Vector3 forward = -transform.up;
        Vector3 size = transform.localScale;

        RaycastHit2D hit = Physics2D.Raycast(carPosition, forward, size.y, LayerMask.GetMask("Car"));
        if (hit.collider != null)
        {
            if (hit.transform.TryGetComponent<CarMovement>(out _))
            {
                Debug.DrawRay(carPosition, forward * size.y, Color.red);
                return true;
            }
        }
        Debug.DrawRay(carPosition, forward * size.y, Color.green);
        return false;
    }

    protected void ReachedDestination()
    {
        if (_pool != null)
        {
            _pool.Release(gameObject);
        }
        else
        {
            Debug.Log("No pool", this);
            Destroy(gameObject);
        }
    }
}
