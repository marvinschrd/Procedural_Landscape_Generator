using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

// Class to generate map in editor without having to start game or project
[CustomEditor(typeof(HeightMapGenerator))]
public class EditorMapGenerator : Editor
{
    public override void OnInspectorGUI()
    {
        HeightMapGenerator mapGenerator2 = (HeightMapGenerator)target;
        // allow for auto update if auto update is selected and value are changed
        if (DrawDefaultInspector())
        {
            //Debug.Log(mapGenerator2);
            if (mapGenerator2.autoUpdateMap)
            {
                mapGenerator2.GenerateMap();
            }
        }

        // Update map on button press
        if (GUILayout.Button("Generate display of map"))
        {
            mapGenerator2.GenerateMap();
        }

        if (GUILayout.Button("Generate random seed map"))
        {
            mapGenerator2.RandomiseValues();
            mapGenerator2.GenerateMap();
        }
        
        if (GUILayout.Button("Generate domain warping map"))
        {
            mapGenerator2.GenerateDomainWarpingMap();
        }
    }
}
