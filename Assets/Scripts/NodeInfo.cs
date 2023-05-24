using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeInfo : MonoBehaviour
{
    public string type;
    public long id;
    public double lat; // still doesnt generate all digits
    public double lon;
    public Dictionary<string, string> tags;
}
