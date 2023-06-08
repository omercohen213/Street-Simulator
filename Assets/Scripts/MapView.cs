using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Collections;
using UnityEngine.Pool;

public class MapView : MonoBehaviour
{
    public static MapView Instance;

    [SerializeField] private GameObject _nodePrefab;
    [SerializeField] private GameObject _predefinedCarPrefab;
    [SerializeField] private GameObject _AICarPrefab;
    private JSONData _mapData;
    public JSONData MapData { get => _mapData; set => _mapData = value; }

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
    }

    List<GameObject> CreateNodes(List<Element> elementNodes)
    {
        List<GameObject> nodes = new();

        foreach (Element elementNode in elementNodes)
        {
            float scale = 10000;
            Vector3 position = new Vector2(elementNode.lon * scale - 133350f, elementNode.lat * scale -525100f);
            
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
        lineRenderer.startWidth = 1f;
        lineRenderer.endWidth = 1f;

        lineRenderer.positionCount = nodes.Count;
        for (int i = 0; i < nodes.Count; i++)
        {
            lineRenderer.SetPosition(i, nodes[i].transform.position);
        }
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
            List<long> startingNodesID = new() { 10074389312 , 4782446448, 2715786143, 290148082 };
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