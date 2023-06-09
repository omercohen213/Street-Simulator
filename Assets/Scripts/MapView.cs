using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Collections;
using UnityEngine.Pool;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using TMPro;

public class MapView : MonoBehaviour
{
    public static MapView Instance;

    [SerializeField] private GameObject _nodePrefab;
    [SerializeField] private GameObject _predefinedCarPrefab;
    [SerializeField] private GameObject _AICarPrefab;
    private JSONData _mapData;
    public JSONData MapData { get => _mapData; set => _mapData = value; }

    private List<Element> _elementNodes;
    private List<Element> _ways;

    ObjectPool<GameObject> _AICarsPool;
    ObjectPool<GameObject> _preDefinedCarsPool;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        DrawMap();

        _preDefinedCarsPool = new ObjectPool<GameObject>(OnCreatePredefinedCar, OnCarGet, OnCarRelease, OnCarDestroy);
        StartCoroutine(CreatePredefinedCars());

        _AICarsPool = new ObjectPool<GameObject>(OnCreateAICar, OnCarGet, OnCarRelease, OnCarDestroy);
        StartCoroutine(CreateAICars());
    }

    private void DrawMap()
    {
        string dataString = File.ReadAllText("Assets/Resources/data.json");
        _mapData = JsonConvert.DeserializeObject<JSONData>(dataString);

        _elementNodes = new List<Element>();
        _ways = new List<Element>();

        foreach (Element element in _mapData.elements)
        {
            //Debug.Log(element.ToString());
            if (element.type == "node")
            {
                _elementNodes.Add(element);
            }
            else if (element.type == "way")
            {
                _ways.Add(element);
            }
        }

        List<GameObject> nodesGo = CreateNodes();
        ConnectNodesWithLines(nodesGo);
    }

    List<GameObject> CreateNodes()
    {
        List<GameObject> nodes = new();

        foreach (Element elementNode in _elementNodes)
        {
            float scale = 10000;
            Vector3 position = new Vector2(elementNode.lon * scale - 133350f, elementNode.lat * scale - 525100f);
            GameObject node = Instantiate(_nodePrefab, position, Quaternion.identity, transform.Find("Nodes"));

            SpriteRenderer nodeSpriteRenderer = node.GetComponent<SpriteRenderer>();
            nodeSpriteRenderer.sortingLayerName = "Road";
            nodeSpriteRenderer.sortingOrder = 1;

            // Add node info
            DataElementInfo nodeInfo = node.GetComponent<DataElementInfo>();
            nodeInfo.type = elementNode.type;
            nodeInfo.id = elementNode.id;
            nodeInfo.lat = elementNode.lat;
            nodeInfo.lon = elementNode.lon;
            nodeInfo.tags = new List<ElementTag>();
            if (elementNode.tags != null)
            {
                foreach (KeyValuePair<string, JToken> pair in elementNode.tags)
                {
                    ElementTag elementTag = new()
                    {
                        tagName = pair.Key,
                        value = pair.Value.ToString()
                    };
                    nodeInfo.tags.Add(elementTag);
                }
            }
            nodes.Add(node);
        }
        return nodes;
    }
        
    private void ConnectNodesWithLines(List<GameObject> nodes)
    {
        foreach (Element way in _ways)
        {
            List<GameObject> nodesToConnect = new();

            foreach (long nodeId in way.nodeIds)
            {
                GameObject node = nodes.Find(x => x.GetComponent<DataElementInfo>().id == nodeId);
                if (node != null)
                {
                    nodesToConnect.Add(node);
                }
            }

            if (nodesToConnect.Count > 1)
            {
                CreateLineRenderer(nodesToConnect, way);
            }
        }
    }

    private void CreateLineRenderer(List<GameObject> nodesToConnect, Element way)
    {
        GameObject wayGo = new("Way");
        wayGo.transform.SetParent(transform.Find("Ways"));

        LineRenderer lineRenderer = wayGo.AddComponent<LineRenderer>();
        lineRenderer.startColor = Color.gray;
        lineRenderer.endColor = Color.gray;
        Material defaultMaterial = new(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        lineRenderer.material = defaultMaterial;
        lineRenderer.startWidth = 1f;
        lineRenderer.endWidth = 1f;
        lineRenderer.sortingLayerName = "Road";
        lineRenderer.sortingOrder = 0;

        lineRenderer.positionCount = nodesToConnect.Count;
        for (int i = 0; i < nodesToConnect.Count; i++)
        {
            lineRenderer.SetPosition(i, nodesToConnect[i].transform.position);
        }

        // Add way info
        DataElementInfo wayInfo = wayGo.AddComponent<DataElementInfo>();
        wayInfo.type = way.type;
        wayInfo.id = way.id;

        wayInfo.nodesID = new List<long>();
        foreach (long id in way.nodeIds)
        {
            wayInfo.nodesID.Add(id);
        }

        wayInfo.tags = new List<ElementTag>();
        if (way.tags != null)
        {
            foreach (KeyValuePair<string, JToken> pair in way.tags)
            {
                ElementTag elementTag = new()
                {
                    tagName = pair.Key,
                    value = pair.Value.ToString()
                };
                wayInfo.tags.Add(elementTag);
            }
        }

        

        /* int laneCount = wayInfo.GetLaneCountInWay();

         // Calculate the offset for each lane
         float laneWidth = 0.5f;
         float laneOffset = (laneCount - 1) * laneWidth;

         for (int laneIndex = 0; laneIndex < laneCount; laneIndex++)
         {
             // Calculate the lateral offset for the current lane
             float laneLateralOffset = (laneIndex - laneOffset);

             GameObject laneMarkings = new GameObject("LaneMarkings");
             laneMarkings.transform.SetParent(wayGo.transform);

             // Create a new line renderer for the lane markings
             LineRenderer laneRenderer = laneMarkings.AddComponent<LineRenderer>();
             laneRenderer.startColor = Color.red;
             laneRenderer.endColor = Color.red;
             Material defaultMaterial = new(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
             laneRenderer.material = defaultMaterial;

             // Calculate the lane offset for each node position
             Vector3 laneOffsetVector = new Vector3(1, 1) * laneLateralOffset;

             // Set the position count and assign positions for the current lane
             laneRenderer.positionCount = nodesToConnect.Count;

             for (int i = 0; i < nodesToConnect.Count; i++)
             {
                 Vector3 position = nodesToConnect[i].transform.position;
                 // Apply the lane offset to the node position
                 position += laneOffsetVector;
                 laneRenderer.SetPosition(i, position);
             }
         }*/
    }

    private List<List<Element>> CreateCarPaths()
    {
        string dataString = File.ReadAllText("Assets/Resources/paths.json");
        Paths carPathsData = JsonConvert.DeserializeObject<Paths>(dataString);
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
    private GameObject OnCreateAICar()
    {
        GameObject AICarGo = Instantiate(_AICarPrefab, Vector3.zero, Quaternion.identity, transform.Find("Cars/AICars"));
        AICarGo.name = "AICar";
        AICarGo.SetActive(false);

        AICarMovement AICar = AICarGo.GetComponent<AICarMovement>();
        AICar.SetPool(_AICarsPool);

        return AICarGo;
    }

    private GameObject OnCreatePredefinedCar()
    {
        GameObject predefinedCarGo = Instantiate(_predefinedCarPrefab, Vector3.zero, Quaternion.identity, transform.Find("Cars/PredefinedCars"));
        predefinedCarGo.name = "PredefinedCar";
        predefinedCarGo.SetActive(false);

        PredefinedCarMovement predefinedCar = predefinedCarGo.GetComponent<PredefinedCarMovement>();
        predefinedCar.SetPool(_preDefinedCarsPool);

        return predefinedCarGo;
    }

    private void OnCarGet(GameObject CarGo)
    {
        CarGo.SetActive(true);
    }

    private void OnCarRelease(GameObject CarGo)
    {
        CarGo.SetActive(false);
    }

    private void OnCarDestroy(GameObject CarGo)
    {
        Destroy(CarGo);
    }

    private IEnumerator CreatePredefinedCars()
    {
        List<List<Element>> carPaths = CreateCarPaths();
        foreach (var path in carPaths)
        {
            GameObject predefinedCarGo = _preDefinedCarsPool.Get();
            predefinedCarGo.name = "PredefinedCar";
            PredefinedCarMovement predefinedCar = predefinedCarGo.GetComponent<PredefinedCarMovement>();
            predefinedCar.Path = path;
            predefinedCar.StartRoute();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator CreateAICars()
    {
        while (true)
        {
            List<long> startingNodesID = new() { 10074389312, 4782446448, 2715786143, 290148082 };
            for (int i = 0; i < 3; i++)
            {
                GameObject AICarGo = _AICarsPool.Get();
                AICarMovement AICar = AICarGo.GetComponent<AICarMovement>();
                AICar.SetStartingNode(startingNodesID[i]);
                AICar.StartRoute();
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
}