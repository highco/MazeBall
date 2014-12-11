using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum Phase { Play, Refill };
public enum GameMode { RotateOuter, Physics };

public struct Round
{
	public string text;
	public string time;
	public int level;
}

public class App : MonoBehaviour 
{
	public static Round round;

	public Sprite[] spritePrefabs;

	public GameMode gameMode;
	public float keyboardFactor;
	public float accelerometerFactor;
	public GameObject background;
	public Text text;
	public float rotationMinSpeed, rotationMaxSpeed;
	public float rotationThreshold;

	public bool changeToNextColorAfterMatch;
	public int maxLevels;

	[HideInInspector] public Phase phase;
	[HideInInspector] float currentAngle;
	[HideInInspector] float targetAngle;
	[HideInInspector] float startTime;
	[HideInInspector] public int matches;
	[HideInInspector] public int maxMatches;

	void Start () 
	{
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		StartRound();
	}

	void StartRound()
	{
		phase = Phase.Play;
		startTime = Time.time;
		matches = 0;
		maxMatches = FindObjectsOfType<Target>().Length;
	}

	public void GameOver(bool win)
	{
		round.time = text.text;
		if (win)
		{
			round.text = "You rock!";
			round.level++;
		}
		else
		{
			round.text = "Game Over";
		}
		Application.LoadLevel("GameOver");
	}

	float lastTapTime;

	void Update()
	{
		bool touch = (Engine.touches.Count > 0 && Engine.touches[0].state == TouchState.Down);

		UpdateGame();

		if (touch && lastTapTime > Time.time - .25f)
		{
			GameOver(win: false);
		}

		if (touch)
		{
			lastTapTime = Time.time;
		}
	}
	
	void UpdateGame() 
	{
		#if UNITY_EDITOR
			var rotationVector = Engine.anyController.LeftStick;
		#else
			var rotationVector = Input.acceleration * accelerometerFactor;
		#endif

		if(gameMode == GameMode.Physics)
		{
			Physics2D.gravity = rotationVector * keyboardFactor;
		}
		else
		if (gameMode == GameMode.RotateOuter)
		{
			targetAngle = Mathf.Atan2(rotationVector.y, rotationVector.x) * Mathf.Rad2Deg;

			while (targetAngle - currentAngle > 180) targetAngle -= 360;
			while (targetAngle - currentAngle < -180) targetAngle += 360;

			var rotationSpeed = Mathf.Lerp(rotationMinSpeed, rotationMaxSpeed, Mathf.Abs(targetAngle - currentAngle) / 180);
			if (currentAngle < targetAngle - rotationThreshold) currentAngle = Mathf.Min(currentAngle + rotationSpeed * Time.deltaTime, targetAngle); else
			if (currentAngle > targetAngle + rotationThreshold) currentAngle = Mathf.Max(currentAngle - rotationSpeed * Time.deltaTime, targetAngle);

			background.transform.localEulerAngles = new Vector3(0, 0, currentAngle);
		}

		int time = Mathf.FloorToInt(Time.time - startTime);
		text.text = string.Format("{0:D2}:{1:D2}", time / 60, time % 60);
	}
}
