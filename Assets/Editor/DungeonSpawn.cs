using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonSpawner))]
public class DungeonSpawn : Editor
{
    public override void OnInspectorGUI()
    {
        DungeonSpawner dungeonSpawner = target as DungeonSpawner;

        if (DrawDefaultInspector())
        {
            //dungeonSpawner.Create();
        }

        if (GUILayout.Button("Spawn"))
        {
            dungeonSpawner.Spawn();
            //map.GenerateMap();
        }

        base.OnHeaderGUI();

    }

}
