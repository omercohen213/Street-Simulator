using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredefinedCarMovement : CarMovement
{
    private List<Element> _path; // The set of nodes representing the path   
    public List<Element> Path { get => _path; set => _path = value; }


    private void Start()
    {
        // Initialize the car's current node to the first node in the path
        if (_path.Count > 0)
            _currentNode = _path[0];
    }

    private void Update()
    {
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

}
