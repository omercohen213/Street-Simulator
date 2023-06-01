using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;


public class MapView : MonoBehaviour
{
    [SerializeField] private GameObject _nodePrefab;
    [SerializeField] private GameObject _carPrefab;
    private JSONData _mapData;

    void Start()
    {
        Vector3 startingCamPos = new Vector3(13331.2783f, 52509.2031f, -1);
        Camera.main.transform.position = startingCamPos;

        string dataString = File.ReadAllText("Assets/Resources/data.json");
        _mapData = JsonConvert.DeserializeObject<JSONData>(dataString);

        List<Element> elementNodes = new List<Element>();
        List<Element> ways = new();

        foreach (Element element in _mapData.elements)
        {
            //Debug.Log(element.ToString());
            if (element.type == "node")
            {
                elementNodes.Add(element);
            }
            else if (element.type == "way")
            {
                ways.Add(element);
            }
        }

        List<GameObject> nodesGo = CreateNodes(elementNodes);
        ConnectNodesWithLines(nodesGo, ways);

        CreateCars(elementNodes);
    }

    List<GameObject> CreateNodes(List<Element> elementNodes)
    {
        List<GameObject> nodes = new();

        foreach (Element elementNode in elementNodes)
        {
            float scale = 1000;
            Vector3 position = new Vector2(elementNode.lon * scale, elementNode.lat * scale);
            GameObject node = Instantiate(_nodePrefab, position, Quaternion.identity, transform.Find("Nodes"));
            NodeInfo nodeInfo = node.GetComponent<NodeInfo>();
            nodeInfo.type = elementNode.type;
            nodeInfo.id = elementNode.id;
            nodeInfo.lat = elementNode.lat;
            nodeInfo.lon = elementNode.lon;
            /* if (elementNode.tags != null)
             {
                 nodeInfo.tags = elementNode.tags.ToDictionary(kv => kv.Key, kv => kv.Value.ToString()); 
            }*/
            nodes.Add(node);
        }
        return nodes;
    }

    private void ConnectNodesWithLines(List<GameObject> nodes, List<Element> ways)
    {
        foreach (Element way in ways)
        {
            List<GameObject> connectedDots = new List<GameObject>();

            foreach (long nodeId in way.nodeIds)
            {
                GameObject node = nodes.Find(x => x.GetComponent<NodeInfo>().id == nodeId);
                if (node != null)
                {
                    connectedDots.Add(node);
                }
            }

            if (connectedDots.Count > 1)
            {
                CreateLineRenderer(connectedDots);
            }
        }
    }

    private void CreateLineRenderer(List<GameObject> nodes)
    {
        GameObject way = new GameObject("Way");
        way.transform.SetParent(transform.Find("Ways"));

        LineRenderer lineRenderer = way.AddComponent<LineRenderer>();
        //Material defaultMaterial = Resources.GetBuiltinResource<Material>("Default-Material");
        //lineRenderer.material = defaultMaterial;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        lineRenderer.positionCount = nodes.Count;
        for (int i = 0; i < nodes.Count; i++)
        {
            lineRenderer.SetPosition(i, nodes[i].transform.position);
        }
    }

    private List<GameObject> CreateCars(List<Element> elementNodes)
    {
        List<GameObject> cars = new();
        float scale = 1000;

        List<List<Element>> carPaths = CreateCarPaths();
        foreach (var path in carPaths)
        {
            Vector3 position = new Vector3(path[0].lon * scale, path[0].lat * scale, 0f);
            GameObject car = Instantiate(_carPrefab, position, Quaternion.identity);
            car.GetComponent<CarMovement>().Path = path;
            cars.Add(car);
        }

        return cars;
    }

    private List<List<Element>> CreateCarPaths()
    {
        string pathString = File.ReadAllText("Assets/Resources/paths.json");
        Paths carPathsData = JsonConvert.DeserializeObject<Paths>(pathString);
        List<List<Element>> carPaths = new();

        foreach (var path in carPathsData.paths)
        {
            List<Element> carPath = new();
            foreach (var nodeId in path)
            {
                Element pathNode = _mapData.elements.Find(element => element.id == nodeId);
                carPath.Add(pathNode);
                //Debug.Log(pathNode);
            }
            carPaths.Add(carPath);
        }
        return carPaths;
    }

    /*private void MoveCarAlongPath(List<GameObject> cars)
    {
        foreach (GameObject car in cars)
        {
            // Check if the car has reached the destination node
            if (car.path.Count == 0)
            {
                // Handle reaching the destination
                // ...

                return;
            }

            // Get the next node in the path
            Element nextNode = car.path[0];
            car.path.RemoveAt(0);

            // Move the car to the position of the next node
            Vector3 targetPosition = GetPositionFromNode(nextNode);
            StartCoroutine(MoveCarToPosition(car.gameObject, targetPosition));

            // Update the car's current node
            car.currentNode = nextNode;
        }     
    }

    private IEnumerator MoveCarToPosition(GameObject carObject, Vector3 targetPosition)
    {
        // Define the movement speed or duration
        float movementSpeed = 5f;
        float movementDuration = Vector3.Distance(carObject.transform.position, targetPosition) / movementSpeed;

        // Perform the movement over time
        float elapsedTime = 0f;
        Vector3 startPosition = carObject.transform.position;
        while (elapsedTime < movementDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / movementDuration);
            carObject.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        // Ensure the car reaches the exact target position
        carObject.transform.position = targetPosition;
    }*/
}
