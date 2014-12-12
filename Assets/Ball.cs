using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour
{
	const float rotationSpeed = 360f; 
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

	Vector3 dir;
	float targetAngle;

	void Update()
	{
		if (rigidbody2D.velocity.magnitude > .1)
			dir = rigidbody2D.velocity;
		var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		transform.localEulerAngles = new Vector3(0, 0, Mathf.MoveTowardsAngle(transform.localEulerAngles.z, angle - 90, rotationSpeed * Time.deltaTime));
	}
}
