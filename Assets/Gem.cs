using UnityEngine;
using System.Collections;

public class Gem : MonoBehaviour 
{
	public Sprite[] sprites;

	public bool matching;
	public int color;
	public void SetColor(int value) { color = value; GetComponent<SpriteRenderer>().sprite = sprites[value]; }

	bool _fall;
	public bool fall { get { return _fall; } set { _fall = value; GetComponent<Rigidbody2D>().isKinematic = !value; } }

	public float angle;
	public float relativeAngle;
	public float radius;
	Vector3 targetPosition;
	public float speed;

	public override string ToString()
	{
		return string.Format("{0:f1} -> {1:f1}", relativeAngle, targetPosition);
	}

	public void SetPositionWithAngle(float angle, float radius)
	{
		this.angle = angle;
		this.radius = radius;
		targetPosition = transform.localPosition = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius; 
	}

	public void SetTargetPosition(float angle)
	{
		targetPosition = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius; 
	}

	void Start () 
	{
	
	}
	
	void Update () 
	{
		if(targetPosition != transform.localPosition)
		{
			transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, speed * Engine.deltaTime);
		}
	}

}
