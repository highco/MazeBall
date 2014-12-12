using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System;
using System.Text;

public class HighscoreList : MonoBehaviour 
{
	public Text headerText, namesText, scoresText;
	public GameObject highscoreList, wonButtons, lostButtons;
	public InitialsScreen initialsScreen;

	List<ScoreEntry> scoreEntries = new List<ScoreEntry>();
	string filename { get { return Path.Combine(Application.persistentDataPath, App.gameState.levelName + "Scores.txt"); } }

	void Start () 
	{
		ReadScores();
		wonButtons.SetActive(App.gameState.levelWon);
		lostButtons.SetActive(!App.gameState.levelWon);
		touchDownTime = Time.time + 1f;

		if (App.gameState.levelWon)
		{
			highscoreList.SetActive(false);
			initialsScreen.Show(App.gameState.playerName);
			headerText.text = ScoreEntry.timeToString(App.gameState.time);
		}
		else
		{
			highscoreList.SetActive(true);
			DisplayScores(highlightUserScore:false);
		}
	}

	public void Show(string name)
	{
		App.gameState.playerName = name;
		
		AddScore(name, App.gameState.time);
		SaveScores();
		DisplayScores(highlightUserScore: true);
		
		highscoreList.SetActive(true);
	}

	void ReadScores()
	{
		try
		{
			scoreEntries.Clear();
			var text = File.ReadAllText(filename);
			foreach(string line in text.Split('\n'))
			{
				var part = line.Split(' ');
				float time;
				float.TryParse(part[1], NumberStyles.Any, CultureInfo.InvariantCulture, out time);
				scoreEntries.Add(new ScoreEntry { name = part[0], time = time });
			}
		}
		catch(Exception) {}
	}

	void AddScore(string name, float time)
	{
		if (name != "AAA")
		{
			ScoreEntry scoreEntry = null;

			foreach (var entry in scoreEntries)
				if (entry.name == name)
				{
					scoreEntry = entry;
					break;
				}

			if (scoreEntry != null)
			{
				if (time < scoreEntry.time)
				{
					scoreEntry.time = time;
				}
			}
			else
			{
				scoreEntries.Add(new ScoreEntry { name = name, time = time });
			}
		}
		scoreEntries.Sort((a, b) => a.time.CompareTo(b.time));
	}

	void SaveScores()
	{
		var writer = new StreamWriter(File.OpenWrite(filename));
		foreach(var e in scoreEntries)
		{
			writer.WriteLine(e.name + " " + e.time);
		}
		writer.Close();
	}

	void DisplayScores(bool highlightUserScore)
	{
		string names = "";
		string scores = "";
		int i = 0;

		foreach(var e in scoreEntries)
		{
			var scoreString = ScoreEntry.timeToString(e.time);

			if (highlightUserScore && e.name == App.gameState.playerName)
			{
				names += "<color=#000000>" + e.name + "</color>\n";
				scores += "<color=#000000>" + scoreString + "</color>\n";
			}
			else
			{
				names += e.name + "\n";
				scores += scoreString + "\n";
			}

			if (++i > 5) return;
		}

		namesText.text = names;
		scoresText.text = scores;
	}

	float touchDownTime;
	
	void Update () 
	{
		if (highscoreList.activeSelf)
		{
			foreach (var t in Engine.touches)
			{
				if (t.state == TouchState.Down)
				{
					touchDownTime = Time.time;
				}
				else
				if (t.state == TouchState.Up)
				{
					if (Time.time > touchDownTime + 1f)
					{
						App.gameState.level = 0;
						Application.LoadLevel(0);
					}
				}
			}
		}
		/*
		if ((Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Began) || Input.GetMouseButtonDown(0))
			touchDownTime = Time.time;

		if (Time.time > touchDownTime && ((Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Ended) || Input.GetMouseButtonUp(0)))
		{
			if (Time.time < touchDownTime + 1f && App.round.level < Application.loadedLevel)
				Application.LoadLevel(App.round.level);
			else
				Application.LoadLevel(0);
		}
		*/
	}

	public void OnRetry()
	{
		Debug.Log("OnRetry");
		Application.LoadLevel(App.gameState.level);
	}

	public void OnContinue()
	{
		Debug.Log("OnContinue");
		App.gameState.level++;

		if (App.gameState.level >= Application.loadedLevel)
			App.gameState.level = 0;

		Application.LoadLevel(App.gameState.level);
	}
}

class ScoreEntry
{
	public string name;
	public float time;
	public static string timeToString(float time) { return string.Format("{0}:{1:D2}", (int)(time / 60), (int)(time % 60)); }
}

