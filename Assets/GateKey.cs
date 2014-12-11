using UnityEngine;

public class GateKey : MonoBehaviour
{
	public Gate gate;

	void OnTriggerEnter2D (Collider2D collider)
	{
		Debug.Log (collider.name);

		gate.Open ();
		Destroy (gameObject);
	}
}
