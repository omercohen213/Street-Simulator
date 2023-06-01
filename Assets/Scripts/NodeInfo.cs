using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeInfo : MonoBehaviour
{
    public string type;
    public long id;
    public float lat;
    public float lon;
    public Dictionary<string, string> tags;
}
