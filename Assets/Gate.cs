using UnityEngine;

public class Gate : MonoBehaviour
{
	void Start ()
	{
		var renderer = transform.GetComponent<SpriteRenderer> ();
		var color = renderer.color;
		color.a = 0.3f;
		renderer.color = color;
	}

	public void Open ()
	{
		gameObject.SetActive (false);
	}
}
