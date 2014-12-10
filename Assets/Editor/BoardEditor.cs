using System.Collections;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Board))]
public class BoardEditor : Editor 
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		var board = target as Board;
		if(GUILayout.Button("Create level"))
			board.CreateLevel();	
	}
}
