using System.Collections;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Labyrith))]
public class LabyrithEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		var lab = target as Labyrith;
		if (GUILayout.Button("Create"))
			lab.Create();
	}
}
