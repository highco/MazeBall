using UnityEngine;
using System.Collections;

public class Labyrith : MonoBehaviour 
{
	public string[] level;

	public GameObject[] segmentPrefabs;
	public GameObject[] wallPrefabs;
	const int segmentCount = 15;

	public void Create()
	{
		Clear();
		CreateCircle(segmentPrefabs[0], "---------------");
		CreateCircle(wallPrefabs[0],    level[0]);
		CreateCircle(segmentPrefabs[1], level[1]);
		CreateCircle(wallPrefabs[1],    level[2]);
		CreateCircle(segmentPrefabs[2], level[3]);
		CreateCircle(wallPrefabs[2],    level[4]);
		
		var segment = (Instantiate(segmentPrefabs[3]) as GameObject);
		segment.transform.parent = transform;
		segment.transform.localPosition = Vector3.zero;
	}
	
	void CreateCircle(GameObject prefab, string line)
	{
		for(int i=0; i<segmentCount; i++)
			if(i<line.Length && line[i] != ' ')
			{
				var segment = (Instantiate(prefab) as GameObject);
				segment.transform.parent = transform;
				segment.transform.localPosition = Vector3.zero;
				segment.transform.localEulerAngles = new Vector3(0, 0, -360.0f * i / segmentCount);
			}
	}

	void Clear()
	{
		while (transform.childCount != 0)
			DestroyImmediate(transform.GetChild(0).gameObject);
	}
}
