using UnityEngine;
using System.Collections;

public class ControllerDebugger : MonoBehaviour 
{
	public int index;

	Engine engine;
	GameObject left,right,up,down;
	GameObject secondaryLeft,secondaryRight,secondaryUp,secondaryDown;
	GameObject stick,action1,action2;
	TextMesh info;

	void Awake() 
	{
		left = transform.Find("Left").gameObject;
		right = transform.Find("Right").gameObject;
		up = transform.Find("Up").gameObject;
		down = transform.Find("Down").gameObject;
		secondaryLeft = transform.Find("SecondaryLeft").gameObject;
		secondaryRight = transform.Find("SecondaryRight").gameObject;
		secondaryUp = transform.Find("SecondaryUp").gameObject;
		secondaryDown = transform.Find("SecondaryDown").gameObject;
		stick = transform.Find("Stick").gameObject;
		action1 = transform.Find("Action1").gameObject;
		action2 = transform.Find("Action2").gameObject;
		info = GetComponentInChildren<TextMesh>();
	}
	
	void Update() 
	{
		if(Engine.controllers.Count>index)
		{
			Controller c=Engine.controllers[index];
			Direction dir = c.LeftStickDirection;
			Direction secondaryDir = c.LeftStickSecondaryDirection;
			Vector3 leftStick = c.LeftStick;

			stick.transform.localPosition = leftStick;
			left.SetActive(dir == Direction.Left); 
			right.SetActive(dir == Direction.Right); 
			up.SetActive(dir == Direction.Up); 
			down.SetActive(dir == Direction.Down);
			secondaryLeft.SetActive(secondaryDir == Direction.Left); 
			secondaryRight.SetActive(secondaryDir == Direction.Right); 
			secondaryUp.SetActive(secondaryDir == Direction.Up); 
			secondaryDown.SetActive(secondaryDir == Direction.Down);
			action1.SetActive(c.Action1);
			action2.SetActive(c.Action2);
			info.text = string.Format("{0:f2}  {1:f2}", leftStick.x, leftStick.y);
		}
	}
}
