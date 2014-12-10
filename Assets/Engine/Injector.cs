using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public interface IUpdatable
{
	void Update(float deltaTime);
}

public static class Injector
{
	static Dictionary<Type, object> objects = new Dictionary<Type, object>();
	static Dictionary<Type, UnityEngine.Object> prefabs = new Dictionary<Type, UnityEngine.Object>();

	public static void Clear()
	{
		objects.Clear();
		prefabs.Clear();
	}

	public static T Get<T>()
	{
		Type type = typeof(T);
		object obj = null;

		if(!objects.TryGetValue(type, out obj))
		{
			if (type.IsSubclassOf(typeof(MonoBehaviour)))
			{
				obj = GameObject.FindObjectOfType(type);
			}

			if(obj == null)
			{
				objects[type] = obj = Activator.CreateInstance<T>();
			}
		}

		return (T)obj;
	}

	public static T Set<T>(T value)
	{
		Type type = typeof(T);
		objects[type] = value;
		return value;
	}

	public static T CreateFromPrefab<T>(GameObject parent=null, Vector3 position = default(Vector3)) where T:MonoBehaviour
	{
		Type type = typeof(T);
		UnityEngine.Object prefab;

		if(!prefabs.TryGetValue(type, out prefab))
		{
			prefabs[type] = prefab = Resources.Load<UnityEngine.Object>(type.Name);
		}

		var instance = (GameObject.Instantiate(prefab) as GameObject).GetComponent<T>();
		
		if(parent != null)
		{
			instance.transform.parent = parent.transform;
		}
		instance.transform.position = position;

		return instance;
	}

	public static GameObject GetFromScene(string name)
	{
		return GameObject.Find(name);
	}

	public static T GetFromScene<T>() where T:MonoBehaviour
	{
		return GameObject.FindObjectOfType<T>();
	}

	public static void ClearView()
	{
		foreach(var obj in GameObject.FindObjectsOfType<GameObject>())
		{
			if (!obj.CompareTag("Static") && !obj.CompareTag("MainCamera")) 
				GameObject.DestroyImmediate(obj);
		}
	}

	public static T Create<T>() where T:MonoBehaviour
	{
		Type type = typeof(T);
		GameObject go = new GameObject();
		T obj = go.AddComponent<T>();
		objects[type] = obj;
		return obj;
	}
}

