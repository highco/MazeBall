using UnityEngine;
using System.Collections;

public class NotifcationDemo : MonoBehaviour 
{
	void Start () 
	{
	
	}

	float touchDownTime;

	void Update () 
	{
		foreach (var t in Engine.touches)
		{
			if (t.state == TouchState.Down)
			{
				touchDownTime = Time.time;
			}
			else
				if (t.state == TouchState.Up)
				{
					if (Time.time > touchDownTime + 1f)
					{
						App.gameState.level = 0;
						Application.LoadLevel(0);
					}
				}
		}		
	}
}
