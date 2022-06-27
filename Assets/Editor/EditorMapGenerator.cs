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
        HeightMapGenerator mapGenerator = (HeightMapGenerator)target;

        // allow for auto update if auto update is selected and value are changed
        if (DrawDefaultInspector())
        {
            if (mapGenerator.autoUpdateMap)
            {
                mapGenerator.GenerateMap();
            }
        }

        // Update map on button press
        if (GUILayout.Button("Generate display of map"))
        {
            mapGenerator.GenerateMap();
        }

        if (GUILayout.Button("Generate random seed map"))
        {
            mapGenerator.RandomiseValues();
            mapGenerator.GenerateMap();
        }
    }
}
