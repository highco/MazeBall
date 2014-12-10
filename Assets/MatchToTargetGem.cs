using UnityEngine;
using System.Collections;

public class MatchToTargetGem : MonoBehaviour
{
	public int color;
	App app;

	void Start()
	{
		app = FindObjectOfType<App>();
		SetColor(color);
	}

	void SetColor(int color)
	{
		this.color = color;
		GetComponent<SpriteRenderer>().sprite = app.spritePrefabs[color];
	}

	void OnTriggerEnter2D(Collider2D collider)
	{
		var target = collider.gameObject.GetComponent<Target>();
		
		if(target != null && target.color == color)
		{
			Destroy(collider.gameObject);

			if (app.changeToNextColorAfterMatch)
			{
				if (color < 5)
					SetColor(++color);
				else
					app.GameOver();				
			}
			else
			{
				Destroy(gameObject);

				if (++app.matches >= app.maxMatches)
					app.GameOver();
			}
		}
	}
}
