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
		touchDownTime = Time.time;
	}

	float touchDownTime;
	
	void Update () 
	{
		if ((Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Began) || Input.GetMouseButtonDown(0))
			touchDownTime = Time.time;

		if ((Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Ended) || Input.GetMouseButtonUp(0))
		{
			if (Time.time < touchDownTime + 1f && App.round.level < Application.loadedLevel)
				Application.LoadLevel(App.round.level);
			else
				Application.LoadLevel(0);
		}
	}
}
