using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum Phase { Play, Refill };
public enum GameMode { RotateOuter, Physics };
public enum AppMode { Game, GameOver };

public class App : MonoBehaviour 
{
	public Sprite[] spritePrefabs;

	public GameMode gameMode;
	public float keyboardFactor;
	public float accelerometerFactor;
	public GameObject background;
	public Text text, gameOverText;
	public float rotationMinSpeed, rotationMaxSpeed;
	public float rotationThreshold;

	public GameObject gameScreen;
	public GameObject gameOverScreen;

	public Phase phase;
	float currentAngle;
	float targetAngle;
	float startTime;
	AppMode appMode;
	public int matches;
	public int maxMatches;
	public bool changeToNextColorAfterMatch;

	void Start () 
	{
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		StartRound();		
	}

	void StartRound()
	{
		phase = Phase.Play;
		startTime = Time.time;
		gameScreen.active = true;
		gameOverScreen.active = false;
		appMode = AppMode.Game;
		matches = 0;
	}

	public void GameOver()
	{
		gameOverText.text = text.text;
		gameScreen.active = false;
		gameOverScreen.active = true;
		appMode = AppMode.GameOver;
	}

	float lastTapTime;

	void Update()
	{
		bool touch = (Engine.touches.Count > 0 && Engine.touches[0].state == TouchState.Down);

		if (appMode == AppMode.Game)
		{
			UpdateGame();

			if (touch && lastTapTime > Time.time - .25f)
				GameOver();
		}
		else
		if (appMode == AppMode.GameOver)
		{
			if (touch) Application.LoadLevel(0);
		}

		if (touch) lastTapTime = Time.time;
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
