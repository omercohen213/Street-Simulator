using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

[Serializable]
public class JSONData
{
    public float version;
    public string generator;
    public IDictionary<string, JToken> osm3s;
    public List<Element> elements;
}

[Serializable]
public class Element
{
    public string type;
    public long id;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public double lat; // still doesnt generate all digits
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public double lon;
    [JsonProperty("nodes", NullValueHandling = NullValueHandling.Ignore)]
    public List<long> nodeIds;
    [JsonExtensionData, JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public IDictionary<string, JToken> tags;

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
                str += $"{nodeId}, ";
            }
        }

        if (tags != null)
        {
            foreach (KeyValuePair<string, JToken> tag in tags)
            {
                str += $"{tag.Key}: {tag.Value}, ";
            }
        }

        return str;
    }
}






