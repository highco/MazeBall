using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Circle
{
	public GameObject parent;
	public int count;
	public int maxCount;
	public float radius;
	public bool fall;
}

public class Board : MonoBehaviour 
{

	public GameObject gemPrefab;
	public int colors;
	public float distanceThreshold;

	public Circle[] circles;

	List<Gem> gems = new List<Gem>();
	App app;
	Circle outerCircle;

	// Use this for initialization
	void Start () 
	{
		app = FindObjectOfType<App>();
		CreateLevel();
	}
	
	public void CreateLevel()
	{
		foreach (var circle in circles)
		{
			Transform t = circle.parent.transform;
			while (t.childCount != 0)
				DestroyImmediate(t.GetChild(0).gameObject);
		}
		gems.Clear();

		foreach (var circle in circles)
		{
			float angleOffset = - Mathf.PI * (circle.count - 1) / circle.maxCount;
			for (int i = 0; i < circle.count; i++)
			{
				int colorIndex = Random.Range(0, colors);
				float angle = Mathf.PI * 2f * i / circle.maxCount + angleOffset;

				var gem = (Instantiate(gemPrefab) as GameObject).GetComponent<Gem>();
				gem.SetColor(colorIndex);
				gem.fall = circle.fall;
				gem.transform.parent = circle.parent.transform;
				gem.SetPositionWithAngle(angle, circle.radius);
				gems.Add(gem);
			}
		}

		outerCircle = circles[1];
	}

	void FindMatchesWithGem(Gem baseGem)
	{
		int color = baseGem.color;

		foreach(Gem gem in gems)
		{
			if (gem.color == color && !gem.matching && gem != baseGem && Vector2.Distance(gem.transform.localPosition, baseGem.transform.localPosition) <= distanceThreshold)
			{
				gem.matching = true;
				FindMatchesWithGem(gem);
			}
		}
	}

	List<Gem> gemsThatNeedToBeReplaced = new List<Gem>();

	void Update () 
	{
		if (app.phase == Phase.Play)
		{
			foreach (Gem gem in gems)
				gem.matching = false;

			foreach (Gem gem in gems)
				if (gem.fall)
				{
					FindMatchesWithGem(gem);
				}

			gemsThatNeedToBeReplaced.Clear();
			for (int i = gems.Count - 1; i >= 0; i--)
			{
				var gem = gems[i];

				if (gem.matching)
				{
					if (!gem.fall)
					{
						gemsThatNeedToBeReplaced.Add(gem);
					}

					Destroy(gem.gameObject);
					gems.RemoveAt(i);
				}
			}

			if (gemsThatNeedToBeReplaced.Count > 0)
			{
				// Find middle angle
				var baseGem = gemsThatNeedToBeReplaced[0];
				var baseAngle = baseGem.angle;
				float totalRelativeAngle = 0;
				foreach (var gem in gemsThatNeedToBeReplaced)
				{
					gem.relativeAngle = gem.angle;
					while (gem.relativeAngle - baseAngle > 180) gem.relativeAngle -= 360;
					while (gem.relativeAngle - baseAngle < -180) gem.relativeAngle += 360;
					totalRelativeAngle += gem.relativeAngle;
				}
				float middleAngle = totalRelativeAngle / gemsThatNeedToBeReplaced.Count;

				// Calculate relativeAngle for every gem
				foreach (var gem in gems)
					if (!gem.fall)
					{
						gem.relativeAngle = gem.angle - middleAngle;
						while (gem.relativeAngle > 180) gem.relativeAngle -= 360;
						while (gem.relativeAngle < -180) gem.relativeAngle += 360;
					}

				var leftGems = gems.Where(a => !a.fall && a.relativeAngle < 0).OrderBy(a => -a.relativeAngle).ToList();
				var rightGems = gems.Where(a => !a.fall && a.relativeAngle >= 0).OrderBy(a => a.relativeAngle).ToList();

				float delta = (float)360f / outerCircle.maxCount;
				PlaceGemsFromList(leftGems, middleAngle - delta / 2, -delta);
				PlaceGemsFromList(rightGems, middleAngle + delta / 2, delta);

				app.phase = Phase.Refill;
			}
		}
	}

	void PlaceGemsFromList(IEnumerable<Gem> gems, float start, float delta)
	{
		int i = 0;
		foreach (Gem gem in gems)
		{
			gem.SetTargetPosition(start + i * delta);
			i++;
		}
	}
}
