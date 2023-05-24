using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using UnityEditor.Experimental.GraphView;
using System.Xml.Linq;

public class MapView : MonoBehaviour
{
    [SerializeField] GameObject dotPrefab;

    // Start is called before the first frame update
    void Start()
    {
        Camera.main.transform.position = new Vector3(13.335f, 52.51f, -1);

        string jsonString = File.ReadAllText("Assets/Resources/data.json");
        JSONData data = JsonConvert.DeserializeObject<JSONData>(jsonString);
        List<GameObject> dots = new List<GameObject>();

        foreach (Element element in data.elements)
        {
            Debug.Log(element.ToString());
            if (element.type == "node")
            {
                Vector3 position = new Vector2((float)(element.lon), (float)(element.lat));
                GameObject dot = Instantiate(dotPrefab, position, Quaternion.identity);
                dots.Add(dot);
            }
        }

        //ConnectDotsWithLines(dots);
    }

    void ConnectDotsWithLines(List<GameObject> dots)
    {
        Debug.Log(dots.Count);
        if (dots.Count < 2)
        {
            Debug.LogWarning("Cannot connect dots. Insufficient number of dots available.");
            return;
        }

        GameObject lineObj = new GameObject("Line");
        lineObj.transform.SetParent(transform);

        LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();
        //lineRenderer.material = lineMaterial;
        lineRenderer.startWidth = 0.00005f;
        lineRenderer.endWidth = 0.00005f;

        lineRenderer.positionCount = dots.Count;
        for (int i = 0; i < dots.Count; i++)
        {
            lineRenderer.SetPosition(i, dots[i].transform.position);
        }
    }

    /*Vector3 ConvertLatLonToScreenPosition(double latitude, double longitude)
    {
        // Define the latitude and longitude range in your data
        double minLatitude = 52.0;
        double maxLatitude = 54.0;
        double minLongitude = 12.0;
        double maxLongitude = 14.0;

        // Get the normalized values of latitude and longitude within the defined range
        float normalizedLatitude = (float)((latitude - minLatitude) / (maxLatitude - minLatitude));
        float normalizedLongitude = (float)((longitude - minLongitude) / (maxLongitude - minLongitude));

        // Get the screen position based on normalized values
        Vector3 screenPosition = new Vector3(normalizedLongitude * Screen.width, normalizedLatitude * Screen.height, 0.0f);

        // Convert the screen position to world space
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

        return worldPosition;
    }*/
}
