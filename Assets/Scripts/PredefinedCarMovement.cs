using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

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
        if (_currentNode == null)
            return;

        Vector3 carPosition = transform.position;
        Vector3 nodePosition = GetPositionFromNode(_currentNode);
        float distanceToNode = Vector3.Distance(carPosition, nodePosition);
        float stoppingDistance = 0.1f * _speed; // Adjust the stopping distance based on the car's speed

        if (distanceToNode <= stoppingDistance)
        {
            if (_path.Count > 1)
            {
                _path.RemoveAt(0);
                _currentNode = _path[0];
                RotateCarTowardsNode(_currentNode);
            }
            else
            {
                _currentNode = null;
                gameObject.SetActive(false);
            }
        }

        float movementDistance = Time.deltaTime * _speed;
        float clampedDistance = Mathf.Clamp(movementDistance, 0f, distanceToNode);

        Vector3 movementVector = (nodePosition - carPosition).normalized * clampedDistance;
        transform.position += movementVector;
    }



}
