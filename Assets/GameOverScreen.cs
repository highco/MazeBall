using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour 
{
	public Text gameOverText;
	public Text infoText;
	
	void Start () 
	{
		gameOverText.text = App.round.time;
		infoText.text = App.round.text;
		touchDownTime = Time.time + 1f;
	}

	float touchDownTime;
	
	void Update () 
	{
		foreach(var t in Engine.touches)
		{
			if(t.state == TouchState.Down)
			{
				touchDownTime = Time.time;
			}
			else
			if(t.state == TouchState.Up)
			{
				if (Time.time > touchDownTime + 1.5f || App.round.level >= Application.loadedLevel)
					App.round.level = 0;

				Application.LoadLevel(App.round.level);
			}
		}

		/*
		if ((Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Began) || Input.GetMouseButtonDown(0))
			touchDownTime = Time.time;

		if (Time.time > touchDownTime && ((Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Ended) || Input.GetMouseButtonUp(0)))
		{
			if (Time.time < touchDownTime + 1f && App.round.level < Application.loadedLevel)
				Application.LoadLevel(App.round.level);
			else
				Application.LoadLevel(0);
		}
		*/
	}
}
