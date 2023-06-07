using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Networking.Types;

[Serializable]
public class JSONData
{
    public List<Element> elements;
}

[Serializable]
public class Element
{
    public string type;
    public long id;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public float lat;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public float lon;
    [JsonProperty("nodes", NullValueHandling = NullValueHandling.Ignore)]
    public List<long> nodeIds;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public IDictionary<string, JToken> tags;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string color;

    public override string ToString()
    {
        string str = $"type: {type}, id: {id}";

        if (lat != 0)
        {
            str += $", lat: {lat}";
        }

        if (lon != 0)
        {
            str += $", lon: {lon}";
        }

        if (nodeIds != null)
        {
            foreach (long nodeId in nodeIds)
            {
                str += $" {nodeId},";
            }
        }

        if (tags != null)
        {
            foreach (KeyValuePair<string, JToken> tag in tags)
            {
                str += $" {tag.Key}: {tag.Value}, ";
            }
        }

        return str;
    }
}

[Serializable]
public class Paths
{
    public List<List<long>> paths;
}




