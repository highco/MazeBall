using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;


namespace InControl
{
	public sealed class AutoDiscover : Attribute
	{
	}

	public enum InputControlType : short
	{
		// Standardized.
		//
		DPadUp=0,
		DPadDown=1,
		DPadLeft=2,
		DPadRight=3,
		
		LeftStickX=4,
		LeftStickY=5,

		RightStickX=6,
		RightStickY=7,

		LeftTrigger=8,
		RightTrigger=9,

		Action1=4,
		Action2=5,
		Action3=6,
		Action4=7,
		
		LeftStickButton=8,
		RightStickButton=9,
		
		LeftBumper=10,
		RightBumper=11,
		
		// Not standardized, but provided for convenience.
		//
		Back=12,
		Start=13,
		Select=14,
		System=15,
		Pause=16,
		Menu=17,
		TouchPadTap=18,

		TiltX=12,
		TiltY=13,
		TiltZ=14,
		ScrollWheel=15,
		TouchPadXAxis=16,
		TouchPadYAxis=17,
		
/*		
		// Not standardized.
		//
		Analog0,
		Analog1,
		Analog2,
		Analog3,
		Analog4,
		Analog5,
		Analog6,
		Analog7,
		Analog8,
		Analog9,
		Analog10,
		Analog11,
		Analog12,
		Analog13,
		Analog14,
		Analog15,
		Analog16,
		Analog17,
		Analog18,
		Analog19,
		
		Button0,
		Button1,
		Button2,
		Button3,
		Button4,
		Button5,
		Button6,
		Button7,
		Button8,
		Button9,
		Button10,
		Button11,
		Button12,
		Button13,
		Button14,
		Button15,
		Button16,
		Button17,
		Button18,
		Button19,
*/		
		// Internal. Must be last.
		//
		Count 
	}

	public class InputControlMapping
	{
		public class Range
		{
			public static Range Complete = new Range { Minimum = -1.0f, Maximum = 1.0f };
			public static Range Positive = new Range { Minimum =  0.0f, Maximum = 1.0f };
			public static Range Negative = new Range { Minimum = -1.0f, Maximum = 0.0f };
			
			public float Minimum;
			public float Maximum;
		}
		
		
		public string Source;
		public InputControlType Target;
		
		// Invert the final mapped value.
		public bool Invert;
		
		// Button means non-zero value will be snapped to -1 or 1.
		public bool Button;
		
		// Raw inputs won't be range remapped, smoothed or filtered.
		public bool Raw;
		
		public Range SourceRange = Range.Complete;
		public Range TargetRange = Range.Complete;
		
		string handle;
		
		
		public float MapValue( float value )
		{
			float sourceValue;
			float targetValue;
			
			if (Raw)
			{
				targetValue = value;
			}
			else
			{
				if (value < SourceRange.Minimum || value > SourceRange.Maximum)
				{
					return 0.0f;
				}
				
				sourceValue = Mathf.InverseLerp( SourceRange.Minimum, SourceRange.Maximum, value );
				targetValue = Mathf.Lerp( TargetRange.Minimum, TargetRange.Maximum, sourceValue );
			}
			
			if (Button && Mathf.Abs(targetValue) > float.Epsilon)
			{
				targetValue = Mathf.Sign( targetValue );
			}
			
			if (Invert)
			{
				targetValue = -targetValue;
			}
			
			return targetValue;
		}
		
		
		public bool TargetRangeIsNotComplete
		{
			get { return TargetRange != Range.Complete; }
		}
		
		
		public string Handle
		{
			get { return (string.IsNullOrEmpty( handle )) ? Target.ToString() : handle; }
			set { handle = value; }
		}
		
		
		bool IsYAxis
		{
			get
			{
				return Target == InputControlType.LeftStickY   ||
					Target == InputControlType.RightStickY;
			}
		}
	}
	
	public class UnityInputDeviceProfile
	{
		public string Name { get; protected set; }
		public string Meta { get; protected set; }

		public InputControlMapping[] AnalogMappings { get; protected set; }
		public InputControlMapping[] ButtonMappings { get; protected set; }

		protected string[] SupportedPlatforms;
		protected string[] JoystickNames;

		protected string RegexName;

		static HashSet<Type> hideList = new HashSet<Type>();

		float sensitivity;
		float lowerDeadZone;
		float upperDeadZone;


		public UnityInputDeviceProfile()
		{
			Name = "";
			Meta = "";

			sensitivity = 1.0f;
			lowerDeadZone = 0.2f;
			upperDeadZone = 0.9f;
		}


		public float Sensitivity
		{ 
			get { return sensitivity; }
			protected set { sensitivity = Mathf.Clamp01( value ); }
		}


		public float LowerDeadZone
		{ 
			get { return lowerDeadZone; }
			protected set { lowerDeadZone = Mathf.Clamp01( value ); }
		}


		public float UpperDeadZone
		{ 
			get { return upperDeadZone; }
			protected set { upperDeadZone = Mathf.Clamp01( value ); }
		}


		public bool IsSupportedOnThisPlatform
		{
			get
			{
				if (SupportedPlatforms == null || SupportedPlatforms.Length == 0)
				{
					return true;
				}

				string plattform = (SystemInfo.operatingSystem + " " + SystemInfo.deviceModel).ToUpper();

				foreach (var platform in SupportedPlatforms)
				{
					if (plattform.Contains( platform.ToUpper() ))
					{
						return true;
					}
				}

				return false;
			}
		}


		public bool IsJoystick 
		{ 
			get 
			{ 
				return (RegexName != null) || (JoystickNames != null && JoystickNames.Length > 0); 
			} 
		}


		public bool IsNotJoystick
		{ 
			get { return !IsJoystick; } 
		}


		public bool HasJoystickName( string joystickName )
		{
			if (IsNotJoystick)
			{
				return false;
			}

			if (JoystickNames == null)
			{
				return false;
			}

			return JoystickNames.Contains( joystickName, StringComparer.OrdinalIgnoreCase );
		}


		public bool HasRegexName( string joystickName )
		{
			if (IsNotJoystick)
			{
				return false;
			}

			if (RegexName == null)
			{
				return false;
			}

			return Regex.IsMatch( joystickName, RegexName, RegexOptions.IgnoreCase );
		}


		public bool HasJoystickOrRegexName( string joystickName )
		{
			return HasJoystickName( joystickName ) || HasRegexName( joystickName );
		}


		public static void Hide( Type type )
		{
			hideList.Add( type );
		}
		
		
		public bool IsHidden
		{
			get { return hideList.Contains( GetType() ); }
		}


		protected const string Button0 = " button 0";
		protected const string Button1 = " button 1";
		protected const string Button2 = " button 2";
		protected const string Button3 = " button 3";
		protected const string Button4 = " button 4";
		protected const string Button5 = " button 5";
		protected const string Button6 = " button 6";
		protected const string Button7 = " button 7";
		protected const string Button8 = " button 8";
		protected const string Button9 = " button 9";
		protected const string Button10 = " button 10";
		protected const string Button11 = " button 11";
		protected const string Button12 = " button 12";
		protected const string Button13 = " button 13";
		protected const string Button14 = " button 14";
		protected const string Button15 = " button 15";
		protected const string Button16 = " button 16";
		protected const string Button17 = " button 17";
		protected const string Button18 = " button 18";
		protected const string Button19 = " button 19";

		protected const string Analog0 = " analog 0";
		protected const string Analog1 = " analog 1";
		protected const string Analog2 = " analog 2";
		protected const string Analog3 = " analog 3";
		protected const string Analog4 = " analog 4";
		protected const string Analog5 = " analog 5";
		protected const string Analog6 = " analog 6";
		protected const string Analog7 = " analog 7";
		protected const string Analog8 = " analog 8";
		protected const string Analog9 = " analog 9";
		protected const string Analog10 = " analog 10";
		protected const string Analog11 = " analog 11";
		protected const string Analog12 = " analog 12";
		protected const string Analog13 = " analog 13";
		protected const string Analog14 = " analog 14";
		protected const string Analog15 = " analog 15";
		protected const string Analog16 = " analog 16";
		protected const string Analog17 = " analog 17";
		protected const string Analog18 = " analog 18";
		protected const string Analog19 = " analog 19";

		protected const string MouseButton0 = "0";
		protected const string MouseButton1 = "1";
		protected const string MouseButton2 = "2";

		protected const string MouseXAxis = " mouse x";
		protected const string MouseYAxis = " mouse y";
		protected const string MouseScrollWheel = " mouse z";
	}
}

