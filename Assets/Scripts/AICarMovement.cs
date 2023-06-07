using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class AICarMovement : CarMovement
{
    private Dictionary<string, List<long>> _nodeConnections = new();
    private Element _nextNode;
    private Rigidbody2D _rigidbody;


    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        long startingNodeID = 4782446448;
        string dataString = File.ReadAllText("Assets/Resources/nodes_connection.json");
        _nodeConnections = JsonConvert.DeserializeObject<Dictionary<string, List<long>>>(dataString);

        _currentNode = FindNodeFromID(startingNodeID);
        _nextNode = GetNextNode();
    }

    void Update()
    {
        // Move the car towards the next node
        if (_nextNode != null)
        {
            Vector3 carPosition = transform.position;
            //Vector3 nodePosition = GetPositionFromNode(_currentNode);
            Vector3 nextNodePosition = GetPositionFromNode(_nextNode);
            float distanceToNode = Vector3.Distance(carPosition, nextNodePosition);

            // Check if the car has reached the next node
            if (distanceToNode <= 0.1f)
            {
                Debug.Log("ok");
                _currentNode = _nextNode;
                _nextNode = GetNextNode();
                if (_nextNode != null)
                    RotateCarTowardsNode(_nextNode);
            }
            else
            {

                // Move the car towards the current node
                transform.position = Vector3.MoveTowards(carPosition, nextNodePosition, Time.deltaTime * _speed);
            }
        }
        else
        {
            Debug.Log("AICar reached dead end");
            Destroy(gameObject);
        }
    }

    // Get the next node to drive to. If there are many options, randomize one of them.
    private Element GetNextNode()
    {
        List<long> nextNodeOptions = FindConnectionsFromNode(_currentNode);

        if (nextNodeOptions != null && nextNodeOptions.Count > 0)
        {
            long randNodeID = nextNodeOptions[Random.Range(0, nextNodeOptions.Count)];
            Element randOption = FindNodeFromID(randNodeID);
            return randOption;
        }

        return null;
    }

    private Element FindNodeFromID(long nodeId)
    {
        JSONData mapData = MapView.Instance.MapData;
        Element node = mapData.elements.Find(element => element.id == nodeId);
        return node;
    }

    // Returns the IDs of the nodes that connect to the given node
    private List<long> FindConnectionsFromNode(Element node)
    {
        foreach (KeyValuePair<string, List<long>> nodeConnection in _nodeConnections)
        {
            if (nodeConnection.Key == node.id.ToString())
            {
                return nodeConnection.Value;
            }
        }
        return null; // Key not found
    }

    private void PrintAllNodeConnections()
    {
        foreach (KeyValuePair<string, List<long>> nodeConnection in _nodeConnections)
        {
            string node = nodeConnection.Key;
            List<long> connections = nodeConnection.Value;

            Debug.Log("Key: " + node);
            foreach (long value in connections)
            {
                Debug.Log(value);
            }
        }
    }
}
