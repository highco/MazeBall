using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System;
using System.Text;

struct ScoreEntry
{
	public string name;
	public float time;
}

public class GameOverScreen : MonoBehaviour 
{
	public Text headerText, namesText, scoresText;


	List<ScoreEntry> scoreEntries = new List<ScoreEntry>();
	string filename { get { return Path.Combine(Application.persistentDataPath, App.round.levelName + "Scores.txt"); } }

	void Start () 
	{
		//gameOverText.text = App.round.time;
		//infoText.text = App.round.text;
		touchDownTime = Time.time + 1f;

		ReadScores();
		SaveScores();
		DisplayScores();
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
		scoreEntries.Add(new ScoreEntry { name = name, time = time });
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

	void DisplayScores()
	{
		string names = "";
		string scores = "";
		int i = 0;

		foreach(var e in scoreEntries)
		{
			names += e.name + "\n";
			scores += String.Format("{0}:{1:D2}\n", (int)(e.time/60), (int)(e.time % 60));
			if (++i > 5) return;
		}

		namesText.text = names;
		scoresText.text = scores;
	}

	float touchDownTime;
	
	void Update () 
	{
		foreach(var t in Engine.touches)
		{
			if(t.state == TouchState.Down)
			{
				touchDownTime = Time.time;
			}
			else
			if(t.state == TouchState.Up)
			{
				if (Time.time > touchDownTime + 1.5f || App.round.level >= Application.loadedLevel)
					App.round.level = 0;

				Application.LoadLevel(App.round.level);
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
}
