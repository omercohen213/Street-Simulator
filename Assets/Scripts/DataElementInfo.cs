using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

public class DataElementInfo : MonoBehaviour
{
    public string type;
    public long id;
    public float lat;
    public float lon;
    public List<long> nodesID;
    public List<ElementTag> tags;

    public int GetMaxSpeedInWay()
    {
        if (type == "way")
        {
            foreach (ElementTag elementTag in tags)
            {
                if (elementTag.tagName == "maxspeed")
                {
                    return int.Parse(elementTag.value);
                }
            }
        }
        return -1;
    }

    public int GetLaneCountInWay()
    {
        if (type == "way")
        {
            foreach (ElementTag elementTag in tags)
            {
                if (elementTag.tagName == "lanes")
                {
                    return int.Parse(elementTag.value);
                }
            }
        }
        return -1;
    }
}

[Serializable]
public class ElementTag
{
    [HideInInspector]
    public string tagName;
    public string value;
}



[CustomEditor(typeof(DataElementInfo))]
public class DataElementInfoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty typeProperty = serializedObject.FindProperty("type");
        SerializedProperty idProperty = serializedObject.FindProperty("id");
        SerializedProperty latProperty = serializedObject.FindProperty("lat");
        SerializedProperty lonProperty = serializedObject.FindProperty("lon");
        SerializedProperty nodesIDProperty = serializedObject.FindProperty("nodesID");
        SerializedProperty tagsProperty = serializedObject.FindProperty("tags");

        EditorGUILayout.PropertyField(typeProperty);
        EditorGUILayout.PropertyField(idProperty);
        
        DataElementInfo dataElementInfo = (DataElementInfo)target;
        if (dataElementInfo.lat != 0f && dataElementInfo.lon != 0f)
        {
            EditorGUILayout.PropertyField(latProperty);
            EditorGUILayout.PropertyField(lonProperty);
        }

        if (dataElementInfo.type == "way")
        {
            EditorGUILayout.PropertyField(nodesIDProperty);
        }

        EditorGUILayout.PropertyField(tagsProperty, true);

        serializedObject.ApplyModifiedProperties();
    }
}