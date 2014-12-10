using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;

[CustomEditor(typeof(Engine))]
public class EngineInspector : Editor 
{
	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI();
		Engine engine = target as Engine;

		EventStreamAction prevEventStreamAction = engine.eventStreamAction;
		engine.eventStreamAction = (EventStreamAction) GUILayout.SelectionGrid((int)engine.eventStreamAction, new[]{"Record", "Playback"}, 2); 
		if(engine.eventStreamAction != prevEventStreamAction)
		{
			if(engine.eventStreamAction == EventStreamAction.Playback)
			{
				engine.playbackPath = EditorUtility.OpenFilePanel("Select EventStream", Application.persistentDataPath, "eventstream");
				if(string.IsNullOrEmpty(engine.playbackPath)) engine.eventStreamAction = EventStreamAction.Record;
			}
			else
			{
				engine.playbackPath = "";
			}
		}
		if(!string.IsNullOrEmpty(engine.playbackPath))
			GUILayout.Label(string.Format("EventStream: {0}", Path.GetFileNameWithoutExtension(engine.playbackPath)));
	}
}
