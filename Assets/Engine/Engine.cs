using System.Collections.Generic;
using UnityEngine;
using InControl;
using System.IO;
using System.Linq;

public enum Control {None, LeftStick, RightStick, DPad, DigitalLeftStickOrDPad, AnalogLeftStickOrDPad, Action1, Action2, Action3, Action4, LeftTrigger, RightTrigger, LeftBumper, RightBumper, Start, Back };
public enum EventStreamAction {Record, Playback};
public enum Direction {None, Left, Right, Up, Down};
public enum TouchState {None, Hover, Down, Move, Stationary, Up, Canceled};

public class Touch
{
	public int id;
	public TouchState state;
	public Vector3 position;
}

public class Engine : MonoBehaviour
{
	public string version;
	public List<Control> controls;
	public bool recordTouches;

	[HideInInspector] public string playbackPath;
	[HideInInspector] public EventStreamAction eventStreamAction;

	public static List<Controller> controllers=new List<Controller>();
	public static AnyController anyController=new AnyController();
	BinaryReader reader;
	BinaryWriter writer;
	System.DateTime startTime;
	int exceptionCount=0, warningCount=0;
	int connectedJoysticks=0;

	static public float deltaTime;
	static public float time;

	List<UnityInputDeviceProfile> deviceProfiles = new List<UnityInputDeviceProfile>()
	{
		new Xbox360WinProfile(),
		//new Xbox360MacProfile(),
	};
	void AutoDiscoverDeviceProfiles()
	{
		/*
		foreach (var type in GetType().Assembly.GetTypes()) 
		{
			if(type.IsSubclassOf(typeof(UnityInputDeviceProfile)))
			{
				var deviceProfile = (UnityInputDeviceProfile) System.Activator.CreateInstance( type );
				
				if (deviceProfile.IsSupportedOnThisPlatform)
				{
					Debug.Log( "Adding profile: " + type.Name + " (" + deviceProfile.Name + ")" );
					deviceProfiles.Add( deviceProfile );
				}
				else
				{
					Debug.Log( "Ignored profile: " + type.Name + " (" + deviceProfile.Name + ")" );
				}
			}
		}
		*/
	}

	public void Start()
	{
		AutoDiscoverDeviceProfiles();
		for(int i=0; i<8; i++)
			controllers.Add(new Controller());
		controllers[0].SetKeyControlls(KeyCode.UpArrow, KeyCode.RightArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.Space, KeyCode.LeftAlt);

		startTime = System.DateTime.Now;
		if(eventStreamAction == EventStreamAction.Record)
		{
			writer=new BinaryWriter(new MemoryStream());
			int seed = Random.seed;
			writer.Write(seed);
			Random.seed = seed;
		}
		else
		if(eventStreamAction == EventStreamAction.Playback)
		{
			byte[] buffer=File.ReadAllBytes(playbackPath);
			reader = new BinaryReader(new MemoryStream(buffer));
			int seed = reader.ReadInt32();
			Random.seed = seed;
			//nextEventFrame = reader.ReadInt32();
		}
		time = 0;
		deltaTime = 1f / Screen.currentResolution.refreshRate;
		lastTime = Time.time;
	}

	void OnDestroy()
	{
		if(writer != null)
		{
			byte[] buffer = (writer.BaseStream as MemoryStream).GetBuffer();
			var timeSpan = System.DateTime.Now - startTime;
			string path = string.Format("{0}/{1:dd.MM.yyyy HH.mm} ({5}.{6:d2}) {3}{4}V{2}.eventstream", Application.persistentDataPath, startTime, version, exceptionCount>0 ? exceptionCount+" Exceptions ":"", warningCount>0 ? warningCount+" Warnings ":"", (int)timeSpan.TotalMinutes, (int)timeSpan.Seconds);
			File.WriteAllBytes(path, buffer);
		}
	}

	int frame = 0;
	//int nextEventFrame=-1;
	enum EventType {Controller, Touch, Acceleration, Key, AddController};
	float lastTime;

	public static List<Touch> touches = new List<Touch>();

	public void Update()
	{
		StartFrame();

		if(writer != null) Record(writer); else
		if(reader != null) Playback(reader); else
			               time = Time.time;

		frame++;
		//time = deltaTime*frame;
		deltaTime = time - lastTime;
		lastTime = time;
	}

	void Record(BinaryWriter stream)
	{
		RecordAddController(stream);
		RecordControllers(stream);
		
		if(recordTouches)
		{
			RecordMouse(stream);
		}

		time = Time.time;
		stream.Write(time);
	}

	bool prevButtonState = false;

	void StartFrame()
	{
		foreach (Touch t in touches)
			t.state = TouchState.Stationary;
	}

	void RecordMouse(BinaryWriter stream)
	{
		const int index = 0;
		for (int i = touches.Count; i < index + 1; i++)
			touches.Add(new Touch());

		bool buttonState = Input.GetMouseButton(0);
		Touch touch = touches[index];

		if(Input.mousePosition != touch.position || buttonState != prevButtonState)
		{
			touch.id = 0;
			touch.position = Input.mousePosition;
			
			if( buttonState && !prevButtonState) touch.state = TouchState.Down; else 
			if( buttonState &&  prevButtonState) touch.state = TouchState.Move; else 
			if(!buttonState &&  prevButtonState) touch.state = TouchState.Up; else 
												 touch.state = TouchState.Hover;

			WriteEventHeader(stream, frame, EventType.Touch, 0);
			writer.Write((byte)touch.state);
			writer.Write((byte)touch.id);
			writer.Write((short)touch.position.x);
			writer.Write((short)touch.position.y);
			
			prevButtonState = buttonState;
		}
	}

	private void RecordControllers(BinaryWriter stream)
	{
		foreach (Controller controller in controllers)
		{
			controller.StartFrame();
			bool changed = controller.UpdateFromController();
			if (changed)
			{
				WriteEventHeader(stream, frame, EventType.Controller, controller.Index);
				controller.Write(stream);
			}
		}
	}

	private void RecordAddController(BinaryWriter stream)
	{
		var names = Input.GetJoystickNames();
		for (int i = connectedJoysticks; i < names.Length; i++)
		{
			string name = names[i];
			UnityInputDeviceProfile profile = deviceProfiles.Find(a => a.HasJoystickName(name));
			if (profile == null) profile = deviceProfiles.Find(a => a.HasRegexName(name));
			if (profile == null) profile = new UnityUnknownDeviceProfile(name);
			//var controller = new Controller(i, profile, controls);
			//controllers.Add(controller);
			controllers[i].ConnectJoystick(i, profile, controls);

			WriteEventHeader(stream, frame, EventType.AddController, i);
			controllers[i].WriteAddController(stream);
		}
		connectedJoysticks = names.Length;
	}

	void WriteEventHeader(BinaryWriter stream, int frame, EventType type, int index)
	{
		//stream.Write(frame);
		stream.Write(float.MinValue);
		stream.Write((byte)(type + (index << 4)));
	}

	void Playback(BinaryReader stream)
	{
		float value;
		foreach(Controller c in controllers)
			c.StartFrame();
		try
		{
			while((value = stream.ReadSingle()) == float.MinValue)
			{
				byte typeByte = stream.ReadByte();
				EventType type = (EventType)(typeByte & 0xf);
				int index = typeByte >> 4;

				switch(type)
				{
					case EventType.AddController:
						if(index >= controllers.Count)
							Debug.LogError(string.Format("AddController event for controller {0}, but only {1} controllers are present", index, controllers.Count));
						controllers[index].ConnectJoystick(index, stream);
						break;
					
					case EventType.Controller:
						if(index >= controllers.Count)
							Debug.LogError(string.Format("Controller event of controller {0}, but only {1} controllers are present", index, controllers.Count)); 
						var controller = controllers[index];
						controller.Read(stream);
						break;

					case EventType.Touch:
						for (int i = touches.Count; i < index + 1 ; i++)
							touches.Add(new Touch());
						Touch touch = touches[index];
						touch.state = (TouchState)reader.ReadByte();
						touch.id = reader.ReadByte();
						touch.position = new Vector3(reader.ReadInt16(), reader.ReadInt16());
						break;
				}

				//nextEventFrame = stream.ReadInt32();
			}
			time = value;
		}
		catch(EndOfStreamException)
		{
			Debug.Log("End of event stream");
			reader = null;
		}
	}
}

public struct TrackedControl
{
	public short index;
	public short streamPosition;
	public string name;
	public InputControlMapping mapping;
	
	public TrackedControl(BinaryReader stream)
	{
		index = stream.ReadInt16();
		streamPosition = stream.ReadInt16();
		name = "";
		mapping = null;
	}

	public void Write(BinaryWriter stream)
	{
		stream.Write(index);
		stream.Write(streamPosition);
	}

}

public class Controller
{
	int index;
	UnityInputDeviceProfile profile;
	List<TrackedControl> analogControls=new List<TrackedControl>();
	List<TrackedControl> buttonControls=new List<TrackedControl>();
	float lowerDeadZone, upperDeadZone;
	KeyCode[] keyCodes;
	//enum DPadMappingMode {None, AnalogToDigital};
	//DPadMappingMode dpadMappingMode;

	public int Index {get{ return index; }}

	public void SetKeyControlls(params KeyCode[] keyCodes)
	{
		this.keyCodes=keyCodes;
	}

	public void ConnectJoystick(int index, UnityInputDeviceProfile profile, List<Control> controls)
	{
		this.index = index;
		this.profile = profile;
		lowerDeadZone = profile.LowerDeadZone;
		upperDeadZone = profile.UpperDeadZone;
		//dpadMappingMode = DPadMappingMode.None;

		int streamPosition=0;
		foreach(var map in profile.AnalogMappings)
		{
			if(   ((map.Target == InputControlType.DPadLeft || map.Target == InputControlType.DPadRight || map.Target == InputControlType.DPadUp || map.Target == InputControlType.DPadDown) && (controls.Contains(Control.DPad) || controls.Contains(Control.AnalogLeftStickOrDPad) || controls.Contains(Control.DigitalLeftStickOrDPad)))
			   || ((map.Target == InputControlType.LeftStickX || map.Target == InputControlType.LeftStickY) && (controls.Contains(Control.LeftStick) || controls.Contains(Control.AnalogLeftStickOrDPad) || controls.Contains(Control.DigitalLeftStickOrDPad)))
			   || ((map.Target == InputControlType.RightStickX || map.Target == InputControlType.RightStickY) && controls.Contains(Control.RightStick))
			){
				analogControls.Add(new TrackedControl(){ index=(short)map.Target, streamPosition=(short)(streamPosition++), mapping=map, name = "joystick " + (index+1) + map.Source });
				//bool analogDPad = (map.Target == InputControlType.DPadLeft || map.Target == InputControlType.DPadRight || map.Target == InputControlType.DPadUp || map.Target == InputControlType.DPadDown);
				//analogControls.Add(new TrackedControl(){ index=(short)map.Target, streamPosition=(short)(analogDPad ? -1 : streamPosition++), mapping=map, name = "joystick " + (index+1) + map.Source });
				//if(analogDPad) dpadMappingMode = DPadMappingMode.AnalogToDigital;
			}
		}

		streamPosition=0;
		foreach(var map in profile.ButtonMappings)
		{
			if(    (map.Target == InputControlType.Action1 && controls.Contains(Control.Action1))
				|| (map.Target == InputControlType.Action2 && controls.Contains(Control.Action2))
				|| (map.Target == InputControlType.Action3 && controls.Contains(Control.Action3))
			   	|| (map.Target == InputControlType.Action4 && controls.Contains(Control.Action4))
				|| (map.Target == InputControlType.Start   && controls.Contains(Control.Start))
			   	|| (map.Target == InputControlType.Back    && controls.Contains(Control.Back))
			   || ((map.Target == InputControlType.DPadLeft || map.Target == InputControlType.DPadRight || map.Target == InputControlType.DPadUp || map.Target == InputControlType.DPadDown) && (controls.Contains(Control.DPad) || controls.Contains(Control.AnalogLeftStickOrDPad) || controls.Contains(Control.DigitalLeftStickOrDPad)))
			){
				buttonControls.Add(new TrackedControl(){ index=(short)map.Target, streamPosition=(short)(1 << (streamPosition++)), mapping=map, name = "joystick " + (index+1) + map.Source });
			}
		}
	}

	public void ConnectJoystick(int index, BinaryReader stream)
	{
		this.index = index;

		int analogCount=stream.ReadInt32();
		for(int i=0; i < analogCount; i++)
			analogControls.Add(new TrackedControl(stream));

		int buttonCount=stream.ReadInt32();
		for(int i=0; i < buttonCount; i++)
			buttonControls.Add(new TrackedControl(stream));
	}
	
	public void WriteAddController(BinaryWriter stream)
	{
		stream.Write(analogControls.Count(a => a.streamPosition > -1));
		foreach(TrackedControl control in analogControls)
			if(control.streamPosition > -1)
				control.Write(stream);

		stream.Write(buttonControls.Count(a => a.streamPosition > -1));
		foreach(TrackedControl control in buttonControls)
			if(control.streamPosition > -1)
				control.Write(stream);
	}
	
	bool[] buttonIsPressed=new bool[19];
	bool[] buttonWasPressed=new bool[19];
	float[] analog=new float[18];
	//Vector2 dpadVector;

	public bool UpdateFromController()
	{
		bool joystickInput = false;
		bool changed = false;
		foreach(TrackedControl control in analogControls)
		{
			float value = Input.GetAxisRaw(control.name);

			if (!control.mapping.Raw)
			{
				/*
				if (control.mapping.TargetRangeIsNotComplete &&
				    Mathf.Abs(value) < Mathf.Epsilon &&
				    Analogs[i].UpdateTime < Mathf.Epsilon)
				{
					// Ignore initial input stream for triggers, because they report
					// zero incorrectly until the value changes for the first time.
					// Example: wired Xbox controller on Mac.
					continue;
				}
				*/

				value = Mathf.InverseLerp(lowerDeadZone, upperDeadZone, Mathf.Abs(value)) * Mathf.Sign(value);
				value = control.mapping.MapValue(value);
				//value = SmoothAnalogValue( value, Analogs[i].LastValue, deltaTime );				
			}
			if(value != 0) 
				joystickInput=true;

			if(!Mathf.Approximately(analog[control.index], value))
			{
				analog[control.index] = value;
				changed = true;
			}
		}

		foreach(TrackedControl control in buttonControls)
		{
			bool isPressed = Input.GetKey(control.name);
			if(isPressed != buttonIsPressed[control.index])
			{
				if(isPressed) buttonWasPressed[control.index] = true;
				changed = true;
			}
			buttonIsPressed[control.index] = isPressed;
		}


		if (!joystickInput && keyCodes!=null)
		{
			float x,y;
			if(Input.GetKey(keyCodes[1])) x= 1; else
			if(Input.GetKey(keyCodes[3])) x=-1; else
										  x= 0;
			if(analog[(int)InputControlType.LeftStickX] != x)
			{
				analog[(int)InputControlType.LeftStickX]=x;
				changed=true;
			}

			if(Input.GetKey(keyCodes[0])) y= 1; else
			if(Input.GetKey(keyCodes[2])) y=-1; else
				                          y= 0;
			if(analog[(int)InputControlType.LeftStickY] != y)
			{
				analog[(int)InputControlType.LeftStickY]=y;
				changed=true;
			}
		}

		/*
		if(dpadMappingMode == DPadMappingMode.AnalogToDigital)
		{
			float threshold = .2f;
			buttonIsPressed[(int)InputControlType.DPadLeft]  = analog[(int)InputControlType.DPadRight] < -threshold;
			buttonIsPressed[(int)InputControlType.DPadRight] = analog[(int)InputControlType.DPadRight] >  threshold;
			buttonIsPressed[(int)InputControlType.DPadDown]  = analog[(int)InputControlType.DPadUp]    < -threshold;
			buttonIsPressed[(int)InputControlType.DPadUp]    = analog[(int)InputControlType.DPadUp]    >  threshold;

		}

		if(buttonIsPressed[(int)InputControlType.DPadLeft])  dpadVector.x = -1; else
		if(buttonIsPressed[(int)InputControlType.DPadRight]) dpadVector.x =  1; else
			                                                 dpadVector.x =  0;

		if(buttonIsPressed[(int)InputControlType.DPadDown])  dpadVector.y = -1; else
		if(buttonIsPressed[(int)InputControlType.DPadUp])    dpadVector.y =  1; else
			                                                 dpadVector.y =  0;
		*/
		return changed;
	}

	public void Write(BinaryWriter stream)
	{
		foreach(TrackedControl control in analogControls)
			if(control.streamPosition > -1)
				stream.Write(analog[control.index]);

		short bits=0;
		foreach(TrackedControl control in buttonControls)
			if(control.streamPosition > -1)
				if(buttonIsPressed[control.index])
					bits |= control.streamPosition;
		stream.Write(bits);
	}

	public void Read(BinaryReader stream)
	{
		foreach(TrackedControl control in analogControls)
			analog[control.index] = stream.ReadSingle();

		short bits=stream.ReadInt16();
		foreach(TrackedControl control in buttonControls)
		{
			bool isPressed = (bits & control.streamPosition) != 0;
			if(isPressed && !buttonIsPressed[control.index])
				buttonWasPressed[control.index] = true;
			buttonIsPressed[control.index] = isPressed;
		}

		//dpadVector = new Vector2(analog[(int)InputControlType.DPadLeft] + analog[(int)InputControlType.DPadRight], analog[(int)InputControlType.DPadDown] + analog[(int)InputControlType.DPadUp]);
	}

	public void StartFrame()
	{
		for(int i=0; i<buttonWasPressed.Length; i++)
			buttonWasPressed[i] = false;
	}

	//const float activAxisThreshold=.8f;
	//const float inactiveAxisThreshold=.5f;
	public const float activAxisThreshold=.5f;
	public const float inactiveAxisThreshold=.5f;

	public Direction LeftStickDirection
	{
		get
		{
			Vector2 vector = LeftStick;
			if(Mathf.Abs(vector.x) >= Mathf.Abs(vector.y))
			{
				if(vector.x < -activAxisThreshold) return Direction.Left;
				if(vector.x >  activAxisThreshold) return Direction.Right;
			}
			else
			{
				if(vector.y < -activAxisThreshold) return Direction.Down;
				if(vector.y >  activAxisThreshold) return Direction.Up;
			}
			return Direction.None;
		}
	}
	
	public Direction LeftStickSecondaryDirection
	{
		get
		{
			Vector2 vector = LeftStick;
			if(Mathf.Abs(vector.x) < Mathf.Abs(vector.y))
			{
				if(vector.x < -activAxisThreshold) return Direction.Left;
				if(vector.x >  activAxisThreshold) return Direction.Right;
			}
			else
			{
				if(vector.y < -activAxisThreshold) return Direction.Down;
				if(vector.y >  activAxisThreshold) return Direction.Up;
			}
			return Direction.None;
		}
	}
	
	public Vector2 LeftStick {get{ return new Vector2(analog[(int)InputControlType.LeftStickX], analog[(int)InputControlType.LeftStickY]); }}
	public Vector2 RightStick {get{ return new Vector2(analog[(int)InputControlType.RightStickX], analog[(int)InputControlType.RightStickY]); }}
	public Vector2 DPad
	{
		get
		{ 
			Vector2 dpadVector;
			float threshold = .5f;
			if(buttonIsPressed[(int)InputControlType.DPadLeft]  || analog[(int)InputControlType.DPadRight] < -threshold) dpadVector.x = -1; else
			if(buttonIsPressed[(int)InputControlType.DPadRight] || analog[(int)InputControlType.DPadRight] >  threshold) dpadVector.x =  1; else
				                                                                                                         dpadVector.x =  0;
			
			if(buttonIsPressed[(int)InputControlType.DPadDown]  || analog[(int)InputControlType.DPadUp]    < -threshold) dpadVector.y = -1; else
			if(buttonIsPressed[(int)InputControlType.DPadUp]    || analog[(int)InputControlType.DPadUp]    >  threshold) dpadVector.y =  1; else
				                                                                                                         dpadVector.y =  0;
			return dpadVector;
		}
	}
	public bool Action1Down {get{ return buttonWasPressed[(int)InputControlType.Action1]; }}
	public bool Action2Down {get{ return buttonWasPressed[(int)InputControlType.Action2]; }}
	public bool Action3Down {get{ return buttonWasPressed[(int)InputControlType.Action3]; }}
	public bool Action4Down {get{ return buttonWasPressed[(int)InputControlType.Action4]; }}
	public bool StartDown   {get{ return buttonWasPressed[(int)InputControlType.Start]; }}
	public bool BackDown    {get{ return buttonWasPressed[(int)InputControlType.Back]; }}
	public bool Action1 {get{ return buttonIsPressed[(int)InputControlType.Action1]; }}
	public bool Action2 {get{ return buttonIsPressed[(int)InputControlType.Action2]; }}
	public bool Action3 {get{ return buttonIsPressed[(int)InputControlType.Action3]; }}
	public bool Action4 {get{ return buttonIsPressed[(int)InputControlType.Action4]; }}
	public bool Start   {get{ return buttonIsPressed[(int)InputControlType.Start]; }}
	public bool Back    {get{ return buttonIsPressed[(int)InputControlType.Back]; }}
}

public class AnyController
{
	public Vector2 LeftStick
	{
		get
		{
			Vector2 vector = Vector2.zero;
			foreach(Controller c in Engine.controllers)
				vector += c.LeftStick;
			return vector;
		}
	}

	public Direction LeftStickDirection
	{
		get
		{
			Vector2 vector = LeftStick;
			if(Mathf.Abs(vector.x) >= Mathf.Abs(vector.y))
			{
				if(vector.x < -Controller.activAxisThreshold) return Direction.Left;
				if(vector.x >  Controller.activAxisThreshold) return Direction.Right;
			}
			else
			{
				if(vector.y < -Controller.activAxisThreshold) return Direction.Down;
				if(vector.y >  Controller.activAxisThreshold) return Direction.Up;
			}
			return Direction.None;
		}
	}

	public bool Action1Down
	{
		get
		{
			foreach(Controller c in Engine.controllers)
				if(c.Action1Down)
					return true;
			return false;
		}
	}

	public bool StartDown
	{
		get
		{
			foreach(Controller c in Engine.controllers)
				if(c.StartDown)
					return true;
			return false;
		}
	}
	
	public bool BackDown
	{
		get
		{
			foreach(Controller c in Engine.controllers)
				if(c.BackDown)
					return true;
			return false;
		}
	}
}

public static class Extensions
{
	public static IList<T> Shuffle<T>(this IList<T> list)
	{
		int n = list.Count;
		while (n > 1)
		{
			n--;
			int k = Random.Range(0, n + 1);
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
		return list;
	}
}
