#region

using UnityEditor;
using UnityEngine;

#endregion

[CustomEditor(typeof(DungeonSpawner))]
public class DungeonSpawn : Editor
{
	#region Public Methods

	public override void OnInspectorGUI()
	{
		var dungeon_spawner = target as DungeonSpawner;

		if (DrawDefaultInspector())
		{
			//dungeonSpawner.Create();
		}

		if (GUILayout.Button("Spawn"))
		{
			dungeon_spawner.Spawn();
			//map.GenerateMap();
		}

		base.OnHeaderGUI();
	}

	#endregion
}