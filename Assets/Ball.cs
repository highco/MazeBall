using UnityEngine;

public class Ball : MonoBehaviour
{
	public int color;
	App app;
	bool _destroyed;

	void Start ()
	{
		app = FindObjectOfType<App> ();
		//SetColor(color);
	}

	void SetColor (int color)
	{
		this.color = color;
		GetComponent<SpriteRenderer> ().sprite = app.spritePrefabs [color];
	}

	void Update ()
	{
		if (_destroyed) {
			var renderer = transform.GetComponent<SpriteRenderer> ();
			var color = renderer.color;
			color.a -= 0.1f;
			renderer.color = color;

			var scale = transform.localScale;
			scale.x = scale.y = scale.x - 0.1f;
			transform.localScale = scale;

			if (color.a <= 0) {
				Destroy (gameObject);
			}
		}
	}

	void OnTriggerEnter2D (Collider2D collider)
	{
		var target = collider.gameObject.GetComponent<Target> ();
		
		if (target != null && target.color == color) {
			if (app.dissolveEffect != null) {
				var dissolve = Instantiate (app.dissolveEffect) as GameObject;
				dissolve.transform.position = transform.position;
			}

			Destroy (collider.gameObject);
			if (app.changeToNextColorAfterMatch) {
				if (color < 5) {
					SetColor (++color);
				} else {
					app.GameOver (won: true);
				}
			} else {
				if (++app.matches >= app.maxMatches) {
					app.GameOver (won: true);
				} else {
					_destroyed = true;
				}
			}
		}
	}
}
