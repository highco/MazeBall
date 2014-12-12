using UnityEngine;
using System.Collections;

public class LookForward : MonoBehaviour 
{
	const float rotationSpeed = 360; 

	Vector3 dir;
	float targetAngle;

	void Start()
	{
		rigidbody2D.fixedAngle = true;
	}

	void Update()
	{
		if (rigidbody2D.velocity.magnitude > .1)
			dir = rigidbody2D.velocity;
		var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		transform.localEulerAngles = new Vector3(0, 0, Mathf.MoveTowardsAngle(transform.localEulerAngles.z, angle - 90, rotationSpeed * Time.deltaTime));
	}
}
