using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Pool;

public class AICarMovement : CarMovement
{
    private Dictionary<string, List<long>> _nodeConnections = new();
    private Element _nextNode;   

    private long _startingNodeID;
    public long StartingNodeID { get => _startingNodeID; set => _startingNodeID = value; }

    void Start()
    {
        string dataString = File.ReadAllText("Assets/Resources/nodes_connection.json");
        _nodeConnections = JsonConvert.DeserializeObject<Dictionary<string, List<long>>>(dataString);


        Element startingNode = FindNodeFromID(_startingNodeID);
        transform.position = GetPositionFromNode(startingNode);
        _currentNode = startingNode;
        _nextNode = GetNextNode();
    }

    void Update()
    {
        if (_nextNode != null)
        {
            Vector3 carPosition = transform.position;
            Vector3 nextNodePosition = GetPositionFromNode(_nextNode);
            float distanceToNode = Vector3.Distance(carPosition, nextNodePosition);
            _moveDir = (nextNodePosition - carPosition).normalized;

            // Check if there is another car in front
            bool isObstacleAhead = DetectObstacleAhead();

            if (!isObstacleAhead)
            {
                //transform.position += _speed * Time.deltaTime * _moveDir; 
                transform.position = Vector3.MoveTowards(carPosition, nextNodePosition, Time.deltaTime * _speed);

                // Check if the car has reached the next node
                if (distanceToNode <= 0.1f)
                {
                    _currentNode = _nextNode;
                    _nextNode = GetNextNode();
                    if (_nextNode != null)
                        RotateCarTowardsNode(_nextNode);
                }
            }
        }
        else
        {
            ReachedDestination();
        }
    }

    private bool DetectObstacleAhead()
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

    public Element FindNodeFromID(long nodeId)
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

    private void ReachedDestination()
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

    public void RestartRoute()
    {
        Element startingNode = FindNodeFromID(_startingNodeID);
        transform.position = GetPositionFromNode(startingNode);
        _currentNode = startingNode;
        _nextNode = GetNextNode();
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
