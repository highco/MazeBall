using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class InitialsScreen : MonoBehaviour
{
	public Text[] initials;
	public float friction;
	public float _deltaBoost;

	Vector2 _startPosition = new Vector2 ();
	float _deltaY = 0;
	float _currentX = 0;
	const float _height = 3313f;
	const int _charCount = 36;
	const float _part = _height / _charCount;

	IList<int> _charIndices = new List<int> {
		0, 0, 0
	};

	string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

	void Start ()
	{
		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		SetInitials ("XM0");

		Debug.Log (">>>>> initials " + GetInitials ());
	}

	void Update ()
	{
		//HandleTouch ();
		HandleMouse ();

		if (_deltaY <= -friction) {
			_deltaY += friction;
		} else if (_deltaY >= friction) {
			_deltaY -= friction;
		} else {
			_deltaY = 0;
		}

		if (_deltaY != 0) {
			//Debug.Log (">>>>> delta " + _deltaY);
		}

		var center = 160f;
		
		if (_currentX > center + 110) {
			Debug.Log (">>>>> ok");
		} else {
			var index = 1;

			if (_currentX < (center - 25)) {
				index = 0;
			} else if (_currentX > (center + 25)) {
				index = 2;
			}

			SetY (index, _deltaY);

			if (_deltaY == 0f) {
				CorrectPosition (index);
			}
		}
	}

	void CorrectPosition (int index)
	{
		var trans = initials [index].transform;
		var selectorPos = trans.localPosition;
		var pos = selectorPos.y;
		var charIndex = Mathf.RoundToInt (pos / _part);

		if (charIndex < 0) {
			charIndex = 0;
		} else if (charIndex >= _charCount) {
			charIndex = _charCount - 1;
		}

		selectorPos.y = charIndex * _part;
		_charIndices [index] = charIndex;
		trans.localPosition = selectorPos;
	}

	void SetInitials (string chars)
	{
		for (int i = 0; i < chars.Length; i++) {
			var c = chars [i];
			var index = _chars.IndexOf (c);
			_charIndices [i] = index;

			var transform = initials [i].transform;
			var pos = transform.localPosition;
			pos.y = index * _part;
			transform.localPosition = pos;
		}
	}

	public string GetInitials ()
	{
		var str = "";

		foreach (var index in _charIndices) {
			str += _chars [index];
		}
	
		return str;
	}

	void HandleTouch ()
	{
		bool hasTouch = Input.touchCount > 0;

		if (hasTouch) {
			var touch = Input.touches [0];

			if (touch.phase == TouchPhase.Began) {
				_startPosition = touch.position;
				_currentX = touch.deltaPosition.x;
			}

			_deltaY = touch.deltaPosition.y;
		}
	}

	void HandleMouse ()
	{
		if (Input.anyKeyDown) {
			_startPosition = Input.mousePosition;
			_currentX = Input.mousePosition.x;
		}

		if (Input.anyKey) {
			_deltaY = Input.mousePosition.y - _startPosition.y;
			_startPosition = Input.mousePosition;
		}
	}

	void SetY (int index, float delta)
	{
		var trans = initials [index].transform;
		var selectorPos = trans.localPosition;
		selectorPos.y += delta;

		if (selectorPos.y < 0) {
			selectorPos.y = 0;
			_deltaY = 0;
		} else if (selectorPos.y > _height) {
			selectorPos.y = _height;
			_deltaY = 0;
		}
		
		trans.localPosition = selectorPos;
	}
}
