using System.Collections.Generic;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    protected Element _currentNode; // The current node the car is at
    protected readonly float _speed = 1f;

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
