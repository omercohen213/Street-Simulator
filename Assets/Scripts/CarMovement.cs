using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    private List<Element> _path; // The set of nodes representing the path
    private Element _currentNode; // The current node the car is at
    private readonly float _speed = 4f;

    public List<Element> Path { get => _path; set => _path = value; }

    private void Start()
    {
        // Initialize the car's current node to the first node in the path
        if (_path.Count > 0)
            _currentNode = _path[0];

        Debug.Log(_path);
    }

    private void Update()
    {
        // Move the car along the path
        MoveCarAlongPath();
    }

    private void MoveCarAlongPath()
    {
        // Check if the car has reached the destination node
        if (_currentNode == null)
            return;

        // Check if the car has reached the current node
        Vector3 carPosition = transform.position;
        Vector3 nodePosition = GetPositionFromNode(_currentNode);
        float distanceToNode = Vector3.Distance(carPosition, nodePosition);
        if (distanceToNode <= 0.1f)
        {
            // Check if there are more nodes in the path
            if (_path.Count > 1)
            {
                // Move to the next node in the path
                _path.RemoveAt(0);
                _currentNode = _path[0];
                RotateCarTowardsNode(_currentNode);
            }
            else
            {

                _currentNode = null;
                // Handle reaching the destination
            }
        }

        // Move the car towards the current node
        transform.position = Vector3.MoveTowards(carPosition, nodePosition, Time.deltaTime * _speed);
    }

    private Vector3 GetPositionFromNode(Element node)
    {
        // Convert the node's latitude and longitude to a Vector3 position
        return new Vector3(node.lon * 1000, node.lat *1000 , 0f);
    }

    private void RotateCarTowardsNode(Element node)
    {
        Vector3 carPosition = transform.position;
        Vector3 nodePosition = GetPositionFromNode(node);
        Vector3 direction = (nodePosition - carPosition).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float newAngle = angle + 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
    }

}
