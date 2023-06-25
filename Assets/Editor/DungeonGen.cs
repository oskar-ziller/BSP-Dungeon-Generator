#region

using UnityEditor;
using UnityEngine;

#endregion

[CustomEditor(typeof(DungeonGenerator))]
public class DunegonGen : Editor
{
	#region Public Methods

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		
		var dungeon_generator = target as DungeonGenerator;

		if (GUILayout.Button("Generate"))
		{
			dungeon_generator.Create();
			//map.GenerateMap();
		}
	}

	#endregion
}