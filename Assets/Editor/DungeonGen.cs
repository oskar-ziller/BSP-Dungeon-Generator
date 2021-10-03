using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonGenerator))]
public class DuneonGen : Editor
{
    public override void OnInspectorGUI()
    {
        DungeonGenerator dungeonGenerator = target as DungeonGenerator;

        if (DrawDefaultInspector())
        {
            dungeonGenerator.CreateGUI();
            //map.GenerateMap();
        }

        if (GUILayout.Button("Generate"))
        {
            dungeonGenerator.CreateGUI();
            //map.GenerateMap();
        }

        base.OnHeaderGUI();
    }
}
