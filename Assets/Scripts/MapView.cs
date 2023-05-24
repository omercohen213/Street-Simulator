using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

public class MapView : MonoBehaviour
{
    [SerializeField] GameObject nodePrefab;

    void Start()
    {
        Vector3 startingCamPos = new Vector3(13331.2783f, 52509.2031f, -1);
        Camera.main.transform.position = startingCamPos;

        string jsonString = File.ReadAllText("Assets/Resources/data.json");
        JSONData data = JsonConvert.DeserializeObject<JSONData>(jsonString);

        List<Element> elementNodes = new List<Element>();
        List<Element> ways = new List<Element>();

        foreach (Element element in data.elements)
        {
            Debug.Log(element.ToString());
            if (element.type == "node")
            {
                elementNodes.Add(element);
            }
            else if (element.type == "way")
            {
                ways.Add(element);
            }
        }

        List<GameObject> nodes = CreateDots(elementNodes);
        ConnectDotsWithLines(nodes, ways);
    }

    List<GameObject> CreateDots(List<Element> elementNodes)
    {
        List<GameObject> nodes = new List<GameObject>();

        foreach (Element elementNode in elementNodes)
        {
            float scale = 1000;
            Vector3 position = new Vector2(elementNode.lon * scale, elementNode.lat * scale);
            GameObject dot = Instantiate(nodePrefab, position, Quaternion.identity, transform.Find("Nodes"));
            NodeInfo nodeInfo = dot.AddComponent<NodeInfo>();
            nodeInfo.type = elementNode.type;
            nodeInfo.id = elementNode.id;
            nodeInfo.lat = elementNode.lat;
            nodeInfo.lon = elementNode.lon;
            /* if (elementNode.tags != null)
             {
                 nodeInfo.tags = elementNode.tags.ToDictionary(kv => kv.Key, kv => kv.Value.ToString()); 
            }*/
            nodes.Add(dot);
        }
        return nodes;
    }

    private void ConnectDotsWithLines(List<GameObject> nodes, List<Element> ways)
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
}
