using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour
{
	public int color;
	App app;

	void Start()
	{
		app = FindObjectOfType<App>();
		//SetColor(color);
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
			if (app.dissolveEffect != null)
			{
				var dissolve = Instantiate(app.dissolveEffect) as GameObject;
				dissolve.transform.position = transform.position;
			}

			Destroy(collider.gameObject);
			if (app.changeToNextColorAfterMatch)
			{
				if (color < 5)
				{
					SetColor(++color);
				}
				else
				{
					app.GameOver(won: true);
				}
			}
			else
			{
				Destroy(gameObject);

				if (++app.matches >= app.maxMatches)
				{
					app.GameOver(won: true);
				}
			}
		}
	}
}
