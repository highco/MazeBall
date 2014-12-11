using UnityEngine;
using System.Collections;

public class Villain : MonoBehaviour 
{
	App app;

	void Start () 
	{
		app = FindObjectOfType<App>();
	}

	void OnCollisionEnter2D(Collision2D collider)
	{
		var gem = collider.gameObject.GetComponent<Ball>();
		if (gem != null)
			app.GameOver(win:false);
	}
}
