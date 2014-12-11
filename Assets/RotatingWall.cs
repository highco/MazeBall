using UnityEngine;

public class RotatingWall : MonoBehaviour
{
	public float speed;

	void Update ()
	{
		var delta = Time.deltaTime * speed;
		transform.Rotate (0, 0, delta);
	}
}
