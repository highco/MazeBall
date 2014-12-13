using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum Phase { Play, Refill };
public enum GameMode { RotateOuter, Physics };

public struct GameState
{
	public string text;
	public float time;
	public int level;
	public string levelName;
	public string playerName;
	public bool levelWon;
}

public class App : MonoBehaviour 
{
	public static GameState gameState;

	public Sprite[] spritePrefabs;

	public GameMode gameMode;
	public float keyboardFactor;
	public float accelerometerFactor;
	public GameObject background;
	public Text text;
	public float rotationMinSpeed, rotationMaxSpeed;
	public float rotationThreshold;
	public GameObject dissolveEffect;

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
		gameState.levelName = Application.loadedLevelName;
	}

	public void GameOver(bool won)
	{
		gameState.levelWon = won;
		Application.LoadLevel("GameOver");
	}

	float touchDownTime;

	void Update()
	{
		foreach (var t in Engine.touches)
		{
			if (t.state == TouchState.Down)
			{
				if (touchDownTime > Time.time - .25f)
				{
					GameOver(won: false);
				}
				touchDownTime = Time.time;
			}
			else
			if (t.state == TouchState.Up)
			{
				//if (Time.time > touchDownTime + 1.5f)
				//	Application.LoadLevel("Notification");
			}
		}

		UpdateGame();
		/*
		bool touch = (Engine.touches.Count > 0 && Engine.touches[0].state == TouchState.Down);

		UpdateGame();

		if (touch && lastTapTime > Time.time - .25f)
		{
			GameOver(won: false);
		}

		if (touch)
		{
			lastTapTime = Time.time;
		}
		 */
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

		float time = Time.time - startTime;
		text.text = string.Format("{0:D2}:{1:D2}", (int)(time / 60), (int)(time % 60));
		gameState.time = time;
	}
}
