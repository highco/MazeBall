using UnityEngine;
using System.Collections;

public class Blinker : MonoBehaviour 
{
	public float timeOn, timeOff;

	float startTime;
	Vector3 startPosition;

	void Start () 
	{
		startTime = Time.time;
		startPosition = transform.localPosition;
	}
	
	void Update () 
	{
		var time = Time.time - startTime;
		float timeInStep = time % (timeOn + timeOff);
		var active = timeInStep < timeOn;
		transform.localPosition = active ? startPosition : new Vector3(1000,0,0);
	}
}
