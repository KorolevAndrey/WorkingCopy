// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using Engine;
using Engine.MathEx;
using Engine.FileSystem;
using System.IO;

// I would like to thank Hellent, SodanKerju, Goto10, and Firefly for all there contributions to this source.

// Notes: (JoystickSliders slider, **JoystickSliderAxes axis **, JoystickAxisFilters filter) hasn't been implemented in the enginapp.dll
// when it becomes available this code should all be just uncommented labeled with //Incin uncomment for 2.0,
// so at this time slider controls are not implemented yet. There is code in this file and the keylistener.cs file

namespace ProjectCommon
{
	[AttributeUsageAttribute( AttributeTargets.Field, AllowMultiple = true )]
	public class DefaultKeyboardMouseValueAttribute : Attribute
	{
		GameControlsManager.SystemKeyboardMouseValue value;

		//

		public DefaultKeyboardMouseValueAttribute( EKeys key )
		{
			value = new GameControlsManager.SystemKeyboardMouseValue( key );
		}

		public DefaultKeyboardMouseValueAttribute( EMouseButtons mouseButton )
		{
			value = new GameControlsManager.SystemKeyboardMouseValue( mouseButton );
		}

        public DefaultKeyboardMouseValueAttribute(MouseScroll mouseScrollDirection)
        {
            value = new GameControlsManager.SystemKeyboardMouseValue(mouseScrollDirection);
        }

		public GameControlsManager.SystemKeyboardMouseValue Value
		{
			get { return value; }
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////

	public enum JoystickAxisFilters
	{
		//NotZero,
		GreaterZero,
		LessZero,
		OnlyGreaterZero,
		OnlyLessZero,
		DEADZONE,
	}

    public enum MouseScroll
    { 
        ScrollUp,
        ScrollDown,
    }

	////////////////////////////////////////////////////////////////////////////////////////////////

	[AttributeUsageAttribute( AttributeTargets.Field, AllowMultiple = true )]
	public class DefaultJoystickValueAttribute : Attribute
	{
		GameControlsManager.SystemJoystickValue value;

		//

		public DefaultJoystickValueAttribute( JoystickButtons button )
		{
			value = new GameControlsManager.SystemJoystickValue( button );
		}

		public DefaultJoystickValueAttribute( JoystickAxes axis, JoystickAxisFilters filter )
		{
			value = new GameControlsManager.SystemJoystickValue( axis, filter );
		}

		public DefaultJoystickValueAttribute( JoystickPOVs pov, JoystickPOVDirections direction )
		{
			value = new GameControlsManager.SystemJoystickValue( pov, direction );
		}

		//public DefaultJoystickValueAttribute(JoystickSliders slider, JoystickSliderAxes axis, JoystickAxisFilters filter) //Incin change for 2.0
		public DefaultJoystickValueAttribute(JoystickSliders slider, JoystickAxes axis, JoystickAxisFilters filter)
		{
			value = new GameControlsManager.SystemJoystickValue(slider, axis,filter);
		}

		public GameControlsManager.SystemJoystickValue Value
		{
			get { return value; }
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////

	public abstract class GameControlsEventData
	{
	}

	////////////////////////////////////////////////////////////////////////////////////////////////

	public abstract class GameControlsKeyEventData : GameControlsEventData
	{
		GameControlKeys controlKey;

		//

		public GameControlsKeyEventData( GameControlKeys controlKey )
		{
			this.controlKey = controlKey;
		}

		public GameControlKeys ControlKey
		{
			get { return controlKey; }
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////

	public class GameControlsKeyDownEventData : GameControlsKeyEventData
	{
		float strength;
        
        public GameControlsKeyDownEventData(GameControlKeys controlKey)
            : base(controlKey)
        {
        }

		public GameControlsKeyDownEventData( GameControlKeys controlKey, float strength )
			: base( controlKey )
		{
			this.strength = strength;
		}

		public float Strength
		{
			get { return strength; }
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////

	public class GameControlsKeyUpEventData : GameControlsKeyEventData
	{
        float strength;

        public GameControlsKeyUpEventData(GameControlKeys controlKey)
            : base(controlKey)
        {
        }

		public GameControlsKeyUpEventData( GameControlKeys controlKey, float strength )
			: base( controlKey )
		{
            this.strength = strength;
		}

        public float Strength
        {
            get { return strength; }
        }
	}

	////////////////////////////////////////////////////////////////////////////////////////////////

	public class GameControlsMouseMoveEventData : GameControlsEventData
	{
		Vec2 mouseOffset;

		public GameControlsMouseMoveEventData( Vec2 mouseOffset )
		{
			this.mouseOffset = mouseOffset;
		}

		public Vec2 MouseOffset
		{
			get { return mouseOffset; }
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////

	public class GameControlsTickEventData : GameControlsEventData
	{
		float delta;

		public GameControlsTickEventData( float delta )
		{
			this.delta = delta;
		}

		public float Delta
		{
			get { return delta; }
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////

	public delegate void GameControlsEventDelegate( GameControlsEventData e );

	////////////////////////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// Represents the player control management.
	/// </summary>
	public sealed class GameControlsManager
	{
		static GameControlsManager instance;
		public static string keyconfig = "user:Configs/Game_Controls.config";
		GameControlItem[] items;
		Dictionary<GameControlKeys, GameControlItem> itemsControlKeysDictionary;

		[Config( "GameControls", "mouseSensitivity" )]
		public static Vec2 mouseSensitivity = new Vec2( 1, 1 );

		[Config( "GameControls", "joystickAxesSensitivity" )]
		public static Vec2 joystickAxesSensitivity = new Vec2( 1, 1 );

		[Config( "GameControls", "alwaysRun" )]
		public static bool alwaysRun = true;

		public static float Deadzone = .20f;

		///////////////////////////////////////////

		public event GameControlsEventDelegate GameControlsEvent;

		///////////////////////////////////////////
		public class SystemControlValue
		{

		}

		public class SystemKeyboardMouseValue : SystemControlValue
		{
			public enum Types
			{
				Key,
				MouseButton,
                MouseScrollDirection,
			}

			Types type;
			EKeys key;
			EMouseButtons mouseButton;
            MouseScroll scrollDirection;
			private GameControlItem _parent;
			public GameControlItem Parent
			{
				get { return _parent; }
				set { _parent = value; }
			}

			public bool Unbound;
			
            public SystemKeyboardMouseValue()
			{
			}

			public SystemKeyboardMouseValue(SystemKeyboardMouseValue source)
			{
				type = source.Type;
				key = source.Key;
				mouseButton = source.MouseButton;
                scrollDirection = source.scrollDirection;
				_parent = source.Parent;
			}

			public SystemKeyboardMouseValue( EKeys key )
			{
				type = Types.Key;
				this.key = key;
			}

			public SystemKeyboardMouseValue( EMouseButtons mouseButton )
			{
				type = Types.MouseButton;
				this.mouseButton = mouseButton;
			}

            public SystemKeyboardMouseValue(MouseScroll mouseScrollDirection)
            {
                type = Types.MouseScrollDirection;
                this.scrollDirection = mouseScrollDirection;
            }

			public Types Type
			{
				get { return type; }
			}

			public EKeys Key
			{
				get { return key; }
			}

			public EMouseButtons MouseButton
			{
				get { return mouseButton; }
			}

            public MouseScroll ScrollDirection
            {
                get { return scrollDirection; }
            }

			public override string ToString()
			{
				if (Unbound)
					return string.Format("{0} - Unbound", Parent.ControlKey);
                if (type == Types.Key)
                    return string.Format("{0} - Key {1}", Parent.ControlKey, key);
                else if (type == Types.MouseScrollDirection)
                    return string.Format("{0} - Scroll {1}", Parent.ControlKey, scrollDirection);
                else
                    return string.Format("{0} - Mouse {1} button", Parent.ControlKey, mouseButton);
			}

            public static void Save(SystemKeyboardMouseValue item, TextBlock block)
            {

                block.SetAttribute("type", item.Type.ToString());
                switch (item.Type)
                {
                    case Types.Key:
                        block.SetAttribute("key", item.Key.ToString());
                        break;
                    case Types.MouseButton:
                        block.SetAttribute("button", item.MouseButton.ToString());
                        break;
                    case Types.MouseScrollDirection:
                        block.SetAttribute("scroll", item.scrollDirection.ToString());
                        break;
                }
            }

            public static SystemKeyboardMouseValue Load(TextBlock block)
            {
                var value = new SystemKeyboardMouseValue();

                var type = block.GetAttribute("type");
                if (!string.IsNullOrEmpty(type))
                    value.type = (Types)Enum.Parse(typeof(Types), type);

                var key = block.GetAttribute("key");
                if (!string.IsNullOrEmpty(key))
                    value.key = (EKeys)Enum.Parse(typeof(EKeys), key);

                var button = block.GetAttribute("button");
                if (!string.IsNullOrEmpty(button))
                    value.mouseButton = (EMouseButtons)Enum.Parse(typeof(EMouseButtons), button);

                var scroll = block.GetAttribute("scroll");
                if (!string.IsNullOrEmpty(scroll))
                    value.scrollDirection = (MouseScroll)Enum.Parse(typeof(MouseScroll), scroll);

                return value;
            }

		}

		///////////////////////////////////////////



		public class SystemJoystickValue : SystemControlValue
		{
			public enum Types
			{
				Button,
				Axis,
				POV,
				Slider,
			}

			Types type;
			JoystickButtons button;
			JoystickAxes axis;
			JoystickAxisFilters axisFilter;
			JoystickPOVs pov;
			JoystickPOVDirections povDirection;
			JoystickSliders slider;
			//JoystickSliderAxes sliderAxis; //Incin change for 2.0
			JoystickAxes sliderAxis; //temporary until 2.0 
			private GameControlItem _parent;

			public GameControlItem Parent
			{
				get { return _parent; }
				set { _parent = value; }
			}

			public bool Unbound;

			public SystemJoystickValue()
			{
			}

			public SystemJoystickValue( JoystickButtons button )
			{
				type = Types.Button;
				this.button = button;
			}

			public SystemJoystickValue(SystemJoystickValue source)
			{
				type = source.Type;
				button = source.Button;
				axis = source.Axis;
				axisFilter = source.AxisFilter;
				pov = source.POV;
				povDirection = source.POVDirection;
				_parent = source.Parent;
			}

			public SystemJoystickValue( JoystickAxes axis, JoystickAxisFilters axisFilter )
			{
				type = Types.Axis;
				this.axis = axis;
				this.axisFilter = axisFilter;
			}

			public SystemJoystickValue( JoystickPOVs pov, JoystickPOVDirections povDirection )
			{
				type = Types.POV;
				this.pov = pov;
				this.povDirection = povDirection;
			}

			//public SystemJoystickValue(JoystickSliders slider, JoystickSliderAxes axe,JoystickAxisFilters filter)//Incin change for 2.0
			public SystemJoystickValue(JoystickSliders slider, JoystickAxes axe,JoystickAxisFilters filter)
			{
				type = Types.Slider;
				this.slider = slider;
				this.sliderAxis = axe;
				this.axisFilter = filter;
			}

			public Types Type
			{
				get { return type; }
			}

			public JoystickButtons Button
			{
				get { return button; }
			}

			public JoystickAxes Axis
			{
				get { return axis; }
			}

			public JoystickAxisFilters AxisFilter
			{
				get { return axisFilter; }
			}

			public JoystickPOVs POV
			{
				get { return pov; }
			}

			public JoystickPOVDirections POVDirection
			{
				get { return povDirection; }
			}


			public JoystickSliders Slider
			{
				get { return slider; }
			}

			//public JoystickSliderAxes SliderAxis //Incin change for 2.0
			public JoystickAxes SliderAxis
			{
				get { return sliderAxis; }
			}

            public static void Save(SystemJoystickValue item, TextBlock block)
            {
                block.SetAttribute("type", item.Type.ToString());
                switch (item.Type)
                {
                    case Types.Button:
                        block.SetAttribute("button", item.Button.ToString());
                        break;
                    case Types.Axis:
                        block.SetAttribute("axis", item.Axis.ToString());
                        block.SetAttribute("axisfilter", item.AxisFilter.ToString());
                        break;
                    case Types.POV:
                        block.SetAttribute("POV", item.POV.ToString());
                        block.SetAttribute("POVDirection", item.POVDirection.ToString());
                        break;
                    case Types.Slider:
                        block.SetAttribute("slider", item.Slider.ToString());
                        block.SetAttribute("sliderAxis", item.SliderAxis.ToString());
                        break;
                }
            }

            public static SystemJoystickValue Load(TextBlock block)
            {
                var value = new SystemJoystickValue();

                {
                    var type = block.GetAttribute("type");
                    if (!string.IsNullOrEmpty(type))
                        value.type = (Types)Enum.Parse(typeof(Types), type);
                }

                {
                    var button = block.GetAttribute("button");
                    if (!string.IsNullOrEmpty(button))
                        value.button = (JoystickButtons)Enum.Parse(typeof(JoystickButtons), button);

                }
                {
                    var axis = block.GetAttribute("axis");
                    if (!string.IsNullOrEmpty(axis))
                        value.axis = (JoystickAxes)Enum.Parse(typeof(JoystickAxes), axis);

                }
                {
                    var axisfilter = block.GetAttribute("axisfilter");
                    if (!string.IsNullOrEmpty(axisfilter))
                        value.axisFilter = (JoystickAxisFilters)Enum.Parse(typeof(JoystickAxisFilters), axisfilter);

                }
                {
                    var pov = block.GetAttribute("POV");
                    if (!string.IsNullOrEmpty(pov))
                        value.pov = (JoystickPOVs)Enum.Parse(typeof(JoystickPOVs), pov);

                }
                {
                    var povdirection = block.GetAttribute("POVDirection");
                    if (!string.IsNullOrEmpty(povdirection))
                        value.povDirection = (JoystickPOVDirections)Enum.Parse(typeof(JoystickPOVDirections), povdirection);

                }
                {
                    var slider = block.GetAttribute("slider");
                    if (!string.IsNullOrEmpty(slider))
                        value.slider = (JoystickSliders)Enum.Parse(typeof(JoystickSliders), slider);

                }
                {
                    var slideraxis = block.GetAttribute("sliderAxis");
                    if (!string.IsNullOrEmpty(slideraxis))
                        //value.sliderAxis = (JoystickSliderAxes)Enum.Parse(typeof(JoystickSliderAxes), slideraxis); // Incin -- uncomment for 2.0 support when available
                        value.sliderAxis = (JoystickAxes)Enum.Parse(typeof(JoystickAxes), slideraxis);

                }
                return value;
            }

			public override string ToString()
			{
				if (Unbound)
					return string.Format("{0} - Unbound", Parent.ControlKey);
				if (type == Types.Axis)
					return string.Format("{0} - Axis: {1}({2})", Parent.ControlKey, Axis, AxisFilter);
				if (type == Types.Button)
					return string.Format("{0} - Button: {1}", Parent.ControlKey, Button);
				if (type == Types.POV)
					return string.Format("{0} - POV: {1}({2})", Parent.ControlKey, POV, POVDirection);
				if (type == Types.Slider)
					return string.Format("{0} - Slider: {1}({2})", Parent.ControlKey, Slider, SliderAxis);
				return "Error";
			}
		}

		///////////////////////////////////////////

		public class GameControlItem
		{
			GameControlKeys controlKey;

			SystemKeyboardMouseValue[] defaultKeyboardMouseValues;
			SystemJoystickValue[] defaultJoystickValues;

			public List<SystemKeyboardMouseValue> bindedKeyboardMouseValues =
			   new List<SystemKeyboardMouseValue>();
			public List<SystemJoystickValue> bindedJoystickValues =
			   new List<SystemJoystickValue>();

			//

			public GameControlItem( GameControlKeys controlKey )
			{
				this.controlKey = controlKey;

				//defaultKeyboardMouseValue
				{
					FieldInfo field = typeof(GameControlKeys).GetField(Enum.GetName(typeof(GameControlKeys), controlKey));
					DefaultKeyboardMouseValueAttribute[] attributes = (DefaultKeyboardMouseValueAttribute[])Attribute.GetCustomAttributes(field, typeof(DefaultKeyboardMouseValueAttribute));

					defaultKeyboardMouseValues = new SystemKeyboardMouseValue[ attributes.Length ];
					for( int n = 0; n < attributes.Length; n++ )
					{
						defaultKeyboardMouseValues[ n ] = attributes[ n ].Value;
						defaultKeyboardMouseValues[n].Parent = this;
					}
				}

				//defaultJoystickValue
				{
					FieldInfo field = typeof(GameControlKeys).GetField(Enum.GetName(typeof(GameControlKeys), controlKey));
					DefaultJoystickValueAttribute[] attributes = (DefaultJoystickValueAttribute[])Attribute.GetCustomAttributes(field, typeof(DefaultJoystickValueAttribute));

					defaultJoystickValues = new SystemJoystickValue[ attributes.Length ];
					for( int n = 0; n < attributes.Length; n++ )
					{
						defaultJoystickValues[ n ] = attributes[ n ].Value;
						defaultJoystickValues[n].Parent = this;
					}
				}
			}

			public GameControlKeys ControlKey
			{
				get { return controlKey; }
			}

			/// <summary>
			/// <b>Don't modify</b>.
			/// </summary>
			public SystemKeyboardMouseValue[] DefaultKeyboardMouseValues
			{
				get { return defaultKeyboardMouseValues; }
			}

			/// <summary>
			/// <b>Don't modify</b>.
			/// </summary>
			public SystemJoystickValue[] DefaultJoystickValues
			{
				get { return defaultJoystickValues; }
			}

			public List<SystemKeyboardMouseValue> BindedKeyboardMouseValues
			{
				get { return bindedKeyboardMouseValues; }
			}

			public List<SystemJoystickValue> BindedJoystickValues
			{
				get { return bindedJoystickValues; }
			}


			public override string ToString()
			{
				if (bindedKeyboardMouseValues.Count > 0)
					return controlKey.ToString() + " - " + bindedKeyboardMouseValues[0].ToString();
				else if (defaultKeyboardMouseValues.Length > 0)
					return controlKey.ToString() + " - " + defaultKeyboardMouseValues[0].ToString();

				return controlKey.ToString() + " - Unbound";
			}
		}

		///////////////////////////////////////////

		/// <summary>
		/// Initialization the class.
		/// </summary>
		/// <returns><b>true</b> if the object successfully initialized; otherwise, <b>false</b>.</returns>
		public static bool Init()
		{
			if( instance != null )
				Log.Fatal( "GameControlsManager class is already initialized." );

			instance = new GameControlsManager();
			bool ret = instance.InitInternal();
			if( !ret )
				Shutdown();
			return ret;
		}

		/// <summary>
		/// Shutdown the class.
		/// </summary>
		public static void Shutdown()
		{
			if( instance != null )
			{
				instance.ShutdownInternal();
				instance = null;
			}
		}

		/// <summary>
		/// Gets an instance of the <see cref="ProjectCommon.GameControlsManager"/>.
		/// </summary>
		public static GameControlsManager Instance
		{
			get { return instance; }
		}

		bool InitInternal()
		{
			//register config settings
			EngineApp.Instance.Config.RegisterClassParameters( typeof( GameControlsManager ) );

			//create items
			{
				int controlKeyCount = 0;
				{
					foreach( object value in Enum.GetValues( typeof( GameControlKeys ) ) )
					{
						GameControlKeys controlKey = (GameControlKeys)value;
						if( (int)controlKey >= controlKeyCount )
							controlKeyCount = (int)controlKey + 1;
					}
				}

				items = new GameControlItem[ controlKeyCount ];
				for( int n = 0; n < controlKeyCount; n++ )
				{
					if( !Enum.IsDefined( typeof( GameControlKeys ), n ) )
					{
						Log.Fatal( "GameControlsManager: Init: Invalid \"GameControlKeys\" enumeration." );
						return false;
					}
					GameControlKeys controlKey = (GameControlKeys)n;
					items[ n ] = new GameControlItem( controlKey );
				}

				string customControlsFile = VirtualFileSystem.GetRealPathByVirtual(keyconfig);
				if (VirtualFile.Exists(customControlsFile))
				{
					LoadCustomConfig();
				}
				else
				{
					ResetKeyMouseSettings();
					ResetJoystickSettings();
					SaveCustomConfig();
				}
			}

			//itemsControlKeysDictionary
			{
				itemsControlKeysDictionary = new Dictionary<GameControlKeys, GameControlItem>();
				foreach( GameControlItem item in items )
				{
					itemsControlKeysDictionary.Add( item.ControlKey, item );
			    }
			}

			return true;
		}

		void ShutdownInternal()
		{
		}

		/// <summary>
		/// Sends the notice on pressing a system key.
		/// </summary>
		/// <param name="e">Key event arguments.</param>
		/// <returns><b>true</b> if such system key is used; otherwise, <b>false</b>.</returns>
		public bool DoKeyDown( KeyEvent e )
		{
			bool handled = false;
			//!!!!!slowly
			foreach( GameControlItem item in items )
			{
				if (item.BindedKeyboardMouseValues.Count > 0)
				{
					foreach (SystemKeyboardMouseValue value in item.BindedKeyboardMouseValues)
				    {
					    if( value.Type == SystemKeyboardMouseValue.Types.Key && value.Key == e.Key )
					    {
						    if( GameControlsEvent != null )
							    GameControlsEvent( new GameControlsKeyDownEventData( item.ControlKey, 1 ) );
						    handled = true;
					    }
				    }
			    }
			}
			return handled;
		}

		/// <summary>
		/// Sends the notice on releasing a system key.
		/// </summary>
		/// <param name="e">Key event arguments.</param>
		/// <returns><b>true</b> if such system key is used; otherwise, <b>false</b>.</returns>
		public bool DoKeyUp( KeyEvent e )
		{
			bool handled = false;
			//!!!!!slowly
			foreach( GameControlItem item in items )
			{
				if (item.BindedKeyboardMouseValues.Count > 0)
				{
					foreach (SystemKeyboardMouseValue value in item.BindedKeyboardMouseValues)
				    {
					    if( value.Type == SystemKeyboardMouseValue.Types.Key && value.Key == e.Key )
					    {
						    if( GameControlsEvent != null )
							    GameControlsEvent( new GameControlsKeyUpEventData( item.ControlKey, 1 ) );
						    handled = true;
					    }
				    }
			    }

			}
			return handled;
		}

		/// <summary>
		/// Sends the notice on pressing a mouse button.
		/// </summary>
		/// <param name="button">A value indicating which button was clicked.</param>
		/// <returns><b>true</b> if such system key is used; otherwise, <b>false</b>.</returns>
		public bool DoMouseDown( EMouseButtons button )
		{
			bool handled = false;
			//!!!!!slowly
			foreach( GameControlItem item in items )
			{
				if (item.BindedKeyboardMouseValues.Count > 0)
				{
					foreach (SystemKeyboardMouseValue value in item.BindedKeyboardMouseValues)
				    {
					    if( value.Type == SystemKeyboardMouseValue.Types.MouseButton &&
						    value.MouseButton == button )
					    {
						    if( GameControlsEvent != null )
							    GameControlsEvent( new GameControlsKeyDownEventData( item.ControlKey, 1 ) );
						    handled = true;
					    }
				    }
			    }
			}
			return handled;
		}

		/// <summary>
		/// Sends the notice on releasing a mouse button.
		/// </summary>
		/// <param name="button">A value indicating which button was clicked.</param>
		/// <returns><b>true</b> if such system key is used; otherwise, <b>false</b>.</returns>
		public bool DoMouseUp( EMouseButtons button )
		{
			bool handled = false;
			//!!!!!slowly
			foreach( GameControlItem item in items )
			{
				if (item.BindedKeyboardMouseValues.Count > 0)
				{
					foreach (SystemKeyboardMouseValue value in item.BindedKeyboardMouseValues)
				    {
					    if( value.Type == SystemKeyboardMouseValue.Types.MouseButton &&
						    value.MouseButton == button )
					    {
						if( GameControlsEvent != null )
							GameControlsEvent( new GameControlsKeyUpEventData( item.ControlKey, 1 ) );
						handled = true;
					    }
				    }
			    }
			} 
			return handled;
		}

		/// <summary>
		/// Sends the notice on cursor moved.
		/// </summary>
		/// <param name="mouseOffset">Current mouse position.</param>
		public void DoMouseMoveRelative( Vec2 mouseOffset )
		{
			if( GameControlsEvent != null )
				GameControlsEvent( new GameControlsMouseMoveEventData( mouseOffset ) );
		}
		
        
        public bool DoMouseMouseWheel(int delta)
        {

            var scrollDirection = delta > 0 ? MouseScroll.ScrollUp : MouseScroll.ScrollDown;

            bool handled = false;
            foreach (GameControlItem item in items)
            {
                if (item.BindedKeyboardMouseValues.Count > 0)
                {
                    foreach (SystemKeyboardMouseValue value in item.BindedKeyboardMouseValues)
                    {
                        if (value.Type == SystemKeyboardMouseValue.Types.MouseScrollDirection &&
                            value.ScrollDirection == scrollDirection)
                        {
                            if (GameControlsEvent != null)
                            {
                                GameControlsEvent(new GameControlsKeyDownEventData(item.ControlKey,delta));
                                GameControlsEvent(new GameControlsKeyUpEventData(item.ControlKey));
                            }
                            handled = true;
                        }
                    }
                }

            }
            return handled;
        }

		
		
        public bool DoJoystickEvent( JoystickInputEvent e )
		{
			//JoystickButtonDownEvent
			{
				JoystickButtonDownEvent evt = e as JoystickButtonDownEvent;
				if( evt != null )
				{
					bool handled = false;
					//!!!!!slowly
					foreach( GameControlItem item in items )
					{

						if (item.BindedJoystickValues.Count > 0)
						{
							foreach (SystemJoystickValue value in item.BindedJoystickValues)
						    {
							    if( value.Type == SystemJoystickValue.Types.Button &&
								    value.Button == evt.Button.Name )
							    {
								    if( GameControlsEvent != null )
								    {
									    GameControlsEvent( new GameControlsKeyDownEventData(
										    item.ControlKey, 1 ) );
								    }
								    handled = true;
							    }
						    }

						}
					}
					return handled;
				}
			}

			//JoystickButtonUpEvent
			{
				JoystickButtonUpEvent evt = e as JoystickButtonUpEvent;
				if( evt != null )
				{
					bool handled = false;
					//!!!!!slowly
					foreach( GameControlItem item in items )
					{

						if (item.BindedJoystickValues.Count > 0)
						{
							foreach (SystemJoystickValue value in item.BindedJoystickValues)
						    {
							    if( value.Type == SystemJoystickValue.Types.Button &&
								    value.Button == evt.Button.Name )
							    {
								    if( GameControlsEvent != null )
									    GameControlsEvent( new GameControlsKeyUpEventData( item.ControlKey, 1 ) );
								    handled = true;
							    }
						    }
					    }
					}
					return handled;
				}
			}

			//JoystickAxisChangedEvent
			{
				JoystickAxisChangedEvent evt = e as JoystickAxisChangedEvent;
				if( evt != null )
				{
					bool handled = false;
					//!!!!!slowly
					foreach( GameControlItem item in items )
					{

						if (item.BindedJoystickValues.Count > 0)
						{
							foreach (SystemJoystickValue value in item.BindedJoystickValues)
						    {
							    if( value.Type == SystemJoystickValue.Types.Axis &&
								    value.Axis == evt.Axis.Name )
							    {
							        float strength = 0f;

								    switch( value.AxisFilter )
								    {
								    case JoystickAxisFilters.LessZero:
										if (evt.Axis.Value < -DeadZone)
										    strength = -evt.Axis.Value;
									    break;

								    case JoystickAxisFilters.GreaterZero:
											    if (evt.Axis.Value > DeadZone)
										    strength = evt.Axis.Value;

											    break;

									case JoystickAxisFilters.OnlyGreaterZero:    //ignore negative values
										    if (evt.Axis.Value >= DeadZone)
												    strength = evt.Axis.Value;
											    break;
										    case JoystickAxisFilters.OnlyLessZero:    //ignore positive values
											    if (evt.Axis.Value <= -DeadZone)
												    strength = -evt.Axis.Value;
									        break;
								    }

								    if( strength != 0 )
								    {
									    if( GameControlsEvent != null )
									    {
										    GameControlsEvent( new GameControlsKeyDownEventData(
											    item.ControlKey, strength ) );
									    }
								    }
								    else
								    {
									    if( GameControlsEvent != null )
									    {
										    GameControlsEvent( new GameControlsKeyUpEventData(
											    item.ControlKey , 1 ) );
									    }
								    }

								    handled = true;
							    }
						    }
					    }
					}

					return handled;
				}
			}

			//JoystickPOVChangedEvent
			{
				JoystickPOVChangedEvent evt = e as JoystickPOVChangedEvent;
				if( evt != null )
				{
					bool handled = false;
					//!!!!!slowly
					foreach( GameControlItem item in items )
					{
						if (item.BindedJoystickValues.Count > 0)
						{
							foreach (SystemJoystickValue value in item.BindedJoystickValues)
						{
							if( value.Type == SystemJoystickValue.Types.POV &&
								value.POV == evt.POV.Name )
							{
								if( ( value.POVDirection & evt.POV.Value ) != 0 )
								{
									if( GameControlsEvent != null )
									{
										GameControlsEvent( new GameControlsKeyDownEventData(
											item.ControlKey, 1 ) );
									}
								}
								else
								{
									if( GameControlsEvent != null )
									{
										GameControlsEvent( new GameControlsKeyUpEventData(
											item.ControlKey, 1 ) );
									}
								}
								handled = true;
							}
						}
					}
					}
					return handled;
				}
			}

			//JoystickSliderChangedEvent
			//Incin -- This code portion isn't implemented in the 2.0 source -- still needs evt.Axe to uncomment..
			// Hopefully Ivan will have time to get around to it soon.
			{
				JoystickSliderChangedEvent evt = e as JoystickSliderChangedEvent;
				if( evt != null )
				{
					bool handled = false;
					//foreach (GameControlItem item in items)
					//{

					//    if (item.BindedJoystickValues.Count > 0)
					//    {
					//        foreach (SystemJoystickValue value in item.BindedJoystickValues)
					//        {
					//            if (value.Type == SystemJoystickValue.Types.Slider &&
					//                value.Slider == evt.Slider.Name && value.SliderAxis==evt.Axe)
					//            {
					//                var currentValue = evt.Axe == JoystickSliderAxes.X
					//                    ? evt.Slider.Value.X
					//                    : evt.Slider.Value.Y;

					//                float strength = 0f;

					//                switch (value.AxisFilter)
					//                {
					//                    case JoystickAxisFilters.LessZero:
					//                        if (currentValue < -DeadZone)
					//                            strength = -currentValue;
					//                        break;

					//                    case JoystickAxisFilters.GreaterZero:
					//                        if (currentValue > DeadZone)
					//                            strength = currentValue;
					//                        break;

					//                    case JoystickAxisFilters.OnlyGreaterZero:    //ignore negative values for foot pedals
					//                        if (currentValue >= 0)
					//                            strength = currentValue;
					//                        break;
					//                    case JoystickAxisFilters.OnlyLessZero:    //ignore positive values for foot pedals
					//                        if (currentValue <= 0)
					//                            strength = -currentValue;
					//                        break;
					//                }

					//                if (strength != 0)
					//                {
					//                    if (GameControlsEvent != null)
					//                    {
					//                        GameControlsEvent(new GameControlsKeyDownEventData(
					//                            item.ControlKey, strength));
					//                    }
					//                }
					//                else
					//                {
					//                    if (GameControlsEvent != null)
					//                    {
					//                        GameControlsEvent(new GameControlsKeyUpEventData(
					//                            item.ControlKey));
					//                    }
					//                }
					//                if (currentValue > DeadZone)
					//                {
					//                    if (GameControlsEvent != null)
					//                    {
					//                        GameControlsEvent(new GameControlsKeyDownEventData(item.ControlKey, currentValue));
					//                    }
					//                }
					//                else
					//                {
					//                    GameControlsEvent(new GameControlsKeyUpEventData(
					//                            item.ControlKey));
					//                }
					//                handled = true;
					//            }
					//        }
					//    }
					//}
					return handled;
				}
			}

			return false;
		}

		public void DoTick( float delta )
		{
			if( GameControlsEvent != null )
				GameControlsEvent( new GameControlsTickEventData( delta ) );
		}

		public void DoKeyUpAll()
		{
			foreach( GameControlItem item in items )
			{
				GameControlsKeyUpEventData eventData =
					new GameControlsKeyUpEventData( item.ControlKey, 1 );

				if( GameControlsEvent != null )
					GameControlsEvent( eventData );
			}
		}

        public float DeadZone
        {
            set { Deadzone = value; }
            get { return Deadzone; }
        }

		public Vec2 MouseSensitivity
		{
			get { return mouseSensitivity; }
			set { mouseSensitivity = value; }
		}

		public Vec2 JoystickAxesSensitivity
		{
			get { return joystickAxesSensitivity; }
			set { joystickAxesSensitivity = value; }
		}

		public bool AlwaysRun
		{
			get { return alwaysRun; }
			set { alwaysRun = value; }
		}

		/// <summary>
		/// Gets the key information collection. <b>Don't modify</b>.
		/// </summary>
		public GameControlItem[] Items
		{
			get { return items; }
		}

		public GameControlItem GetItemByControlKey( GameControlKeys controlKey )
		{
			GameControlItem item;
			if( !itemsControlKeysDictionary.TryGetValue( controlKey, out item ) )
				return null;
			return item;
		}

		public bool IsDefaultControlKey(GameControlItem item, EKeys key)
		{
			if (item == null)
				return false;

			foreach (SystemKeyboardMouseValue value in item.DefaultKeyboardMouseValues)
				if (value.Type == SystemKeyboardMouseValue.Types.Key && value.Key == key)
					return true;

			return false;
		}

		public bool IsDefaultControlKey(GameControlKeys controlKey, EKeys key)
		{
			return IsDefaultControlKey(GetItemByControlKey(controlKey), key);
		}

		public bool IsDefaultControlKey(GameControlItem item, EMouseButtons button)
		{
			if (item == null)
				return false;

			foreach (SystemKeyboardMouseValue value in item.DefaultKeyboardMouseValues)
				if (value.Type == SystemKeyboardMouseValue.Types.MouseButton && value.MouseButton == button)
					return true;

			return false;
		}

		public bool IsDefaultControlKey(GameControlKeys controlKey, EMouseButtons button)
		{
			return IsDefaultControlKey(GetItemByControlKey(controlKey), button);
		}

		public bool IsDefaultControlKey(GameControlItem item, JoystickInputEvent e)
		{
			if (item == null)
				return false;

			foreach (SystemJoystickValue value in item.DefaultJoystickValues)
			{
				JoystickButtonEvent bEvt = e as JoystickButtonEvent;
				if (bEvt != null)
				{
					if (value.Type == SystemJoystickValue.Types.Button &&
						value.Button == bEvt.Button.Name)
						return true;
				}

				JoystickAxisChangedEvent acEvt = e as JoystickAxisChangedEvent;
				if (acEvt != null)
				{
					if (value.Type == SystemJoystickValue.Types.Axis &&
						value.Axis == acEvt.Axis.Name)
					{
						if (value.AxisFilter == JoystickAxisFilters.GreaterZero &&
							acEvt.Axis.Value > 0)
							return true;

						if (value.AxisFilter == JoystickAxisFilters.LessZero &&
							acEvt.Axis.Value < 0)
							return true;

						if (value.AxisFilter == JoystickAxisFilters.OnlyGreaterZero &&
							acEvt.Axis.Value > 0)
							return true;

						if (value.AxisFilter == JoystickAxisFilters.OnlyLessZero &&
							acEvt.Axis.Value < 0)
							return true;
					}
	            }

				JoystickPOVChangedEvent pcEvt = e as JoystickPOVChangedEvent;
				if (pcEvt != null)
				{
					if (value.Type == SystemJoystickValue.Types.POV &&
						value.POV == pcEvt.POV.Name && value.POVDirection == pcEvt.POV.Value)
						return true;
				}
			}

			return false;
		}

		public bool IsDefaultControlKey(GameControlKeys controlKey, JoystickInputEvent e)
		{
			return IsDefaultControlKey(GetItemByControlKey(controlKey), e);
		}

		public bool IsControlKey(GameControlItem item, EKeys key)
		{
			if (item == null)
				return false;

			if (item.BindedKeyboardMouseValues.Count > 0)
			{
				foreach (SystemKeyboardMouseValue value in item.BindedKeyboardMouseValues)
					if (value.Type == SystemKeyboardMouseValue.Types.Key && value.Key == key)
						return true;
			}

			return IsDefaultControlKey(item, key);
		}

		public bool IsControlKey(GameControlKeys controlKey, EKeys key)
		{
			return IsControlKey(GetItemByControlKey(controlKey), key);
		}

		public bool IsControlKey(GameControlItem item, EMouseButtons button)
		{
			if (item == null)
				return false;

			if (item.BindedKeyboardMouseValues.Count > 0)
			{
				foreach (SystemKeyboardMouseValue value in item.BindedKeyboardMouseValues)
					if (value.Type == SystemKeyboardMouseValue.Types.MouseButton && value.MouseButton == button)
						return true;
			}

			return IsDefaultControlKey(item, button);
		}

		public bool IsControlKey(GameControlKeys controlKey, EMouseButtons button)
		{
			return IsControlKey(GetItemByControlKey(controlKey), button);
		}

		public bool IsControlKey(GameControlItem item, JoystickInputEvent e)
		{
			return IsDefaultControlKey(item, e);
        }

		public bool IsControlKey(GameControlKeys controlKey, JoystickInputEvent e)
		{
			return IsControlKey(GetItemByControlKey(controlKey), e);
		}

		public bool IsAlreadyBinded(EKeys key, out SystemKeyboardMouseValue control)
		{
			control = null;
			foreach (GameControlItem item in Items)
			{
				if (item.BindedKeyboardMouseValues.Count > 0)
				{
					foreach (SystemKeyboardMouseValue value in item.BindedKeyboardMouseValues)
						if (value.Type == SystemKeyboardMouseValue.Types.Key && value.Key == key)
						{
							control = value;
							return true;
						}
				}
			}

			return false;
		}

		public bool IsAlreadyBinded(EMouseButtons button, out SystemKeyboardMouseValue control)
		{
			control = null;
			foreach (GameControlItem item in Items)
			{
				if (item.BindedKeyboardMouseValues.Count > 0)
				{
					foreach (SystemKeyboardMouseValue value in item.BindedKeyboardMouseValues)
					{

						if (value.Type == SystemKeyboardMouseValue.Types.MouseButton &&
							value.MouseButton == button)
						{
							control = value;
							return true;
						}
					}
				}
			}
			return false;
		}
        
        public bool IsAlreadyBinded(MouseScroll scrollDirection, out SystemKeyboardMouseValue control)
        {
            control = null;
            foreach (GameControlItem item in Items)
            {
                if (item.BindedKeyboardMouseValues.Count > 0)
                {
                    foreach (SystemKeyboardMouseValue value in item.BindedKeyboardMouseValues)
                    {

                        if (value.Type == SystemKeyboardMouseValue.Types.MouseScrollDirection &&
                            value.ScrollDirection == scrollDirection)
                        {
                            control = value;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

		public bool IsAlreadyBinded(JoystickButtons button, out SystemJoystickValue keyValue)
		{
			keyValue = null;
			foreach (GameControlItem item in Items)
			{
				if (item.BindedJoystickValues.Count > 0)
				{
					foreach (SystemJoystickValue value in item.BindedJoystickValues)
					{
						if (value.Type == SystemJoystickValue.Types.Button && value.Button == button)
						{
							keyValue = value;
							return true;
						}
					}
				}
			}
			return false;
		}

		public bool IsAlreadyBinded(JoystickAxes axis, JoystickAxisFilters filters, out SystemJoystickValue keyValue)
		{
			keyValue = null;
			foreach (GameControlItem item in Items)
			{
				if (item.BindedJoystickValues.Count > 0)
				{
					foreach (SystemJoystickValue value in item.BindedJoystickValues)
					{
						if (value.Type == SystemJoystickValue.Types.Axis && value.Axis == axis &&
							value.AxisFilter == filters)
						{
							keyValue = value;
							return true;
						}
					}
				}
			}
			return false;
		}

		public bool IsAlreadyBinded(JoystickPOVs pov, JoystickPOVDirections dir, out SystemJoystickValue keyValue)
		{
			keyValue = null;
			foreach (GameControlItem item in Items)
			{
				if (item.BindedJoystickValues.Count > 0)
				{
					foreach (SystemJoystickValue value in item.BindedJoystickValues)
					{
						if (value.Type == SystemJoystickValue.Types.POV && value.POV == pov && value.POVDirection == dir)
						{
							keyValue = value;
							return true;
						}
					}
				}
			}
			return false;
		}

		//public bool IsAlreadyBinded(JoystickSliders slider, JoystickSliderAxes axis, out SystemJoystickValue keyValue) //Incin change for 2.0 
		public bool IsAlreadyBinded(JoystickSliders slider, JoystickAxes axis, out SystemJoystickValue keyValue)
		{
			keyValue = null;
			foreach (GameControlItem item in Items)
			{
				if (item.BindedJoystickValues.Count > 0)
				{
					foreach (SystemJoystickValue value in item.BindedJoystickValues)
					{
						if (value.Type == SystemJoystickValue.Types.Slider && value.Slider == slider && value.SliderAxis == axis)
						{
							keyValue = value;
							return true;
						}
					}
				}
			}
			return false;
		}

		public void ResetKeyMouseSettings()
		{
			foreach (GameControlItem item in Items)
			{
				item.BindedKeyboardMouseValues.Clear();

				foreach (var defaultKeyboardMouseValue in item.DefaultKeyboardMouseValues)
				{
					item.BindedKeyboardMouseValues.Add(new SystemKeyboardMouseValue(defaultKeyboardMouseValue));
				}
			}
		}
		
        public void ResetJoystickSettings()
		{
			foreach (GameControlItem item in Items)
			{
				item.BindedJoystickValues.Clear();

				foreach (var defaultJoysticValue in item.DefaultJoystickValues)
				{
					item.BindedJoystickValues.Add(new SystemJoystickValue(defaultJoysticValue));
				}
			}
		}

		public void SaveCustomConfig()
		{
			var block = new TextBlock();
			var controlBloc = block.AddChild("Controls");
           
            //var deadzone = controlBloc.AddChild("DeadZone");
            var keyBlockDz = DeadZone.ToString();
            block.SetAttribute("DeadZone", keyBlockDz);

			foreach (GameControlItem item in Items)
			{
				var currentKeyBlock = controlBloc.AddChild(item.ControlKey.ToString());
				//keybord Setting
				if (item.BindedKeyboardMouseValues.Count > 0)
				{
					var keyboardBlock = currentKeyBlock.AddChild("Keybord");
					foreach (var keyboardvalue in item.BindedKeyboardMouseValues)
					{
						var keyBlock = keyboardBlock.AddChild("Item");
						SystemKeyboardMouseValue.Save(keyboardvalue, keyBlock);
					}
				}
				//Joystick setting
				if (item.BindedJoystickValues.Count > 0)
				{
					var joystickBlock = currentKeyBlock.AddChild("Joystick");
					foreach (var joystickvalue in item.BindedJoystickValues)
					{
						var keyBlock = joystickBlock.AddChild("Item");
						SystemJoystickValue.Save(joystickvalue, keyBlock);
					}
				}
        
			}

			string fileName = VirtualFileSystem.GetRealPathByVirtual(keyconfig);
			try
			{
				string directoryName = Path.GetDirectoryName(fileName);
				if (directoryName != "" && !Directory.Exists(directoryName))
					Directory.CreateDirectory(directoryName);
				using (StreamWriter writer = new StreamWriter(fileName))
				{
                         writer.Write(block.DumpToString());
 				}
			}
			catch
			{
				Log.Fatal(string.Format("Saving file failed \"{0}\".", fileName));
				return;
			}
		}

		public void LoadCustomConfig()
		{
			string error;
			string customFilename = VirtualFileSystem.GetRealPathByVirtual(keyconfig);
			TextBlock customblock = TextBlockUtils.LoadFromRealFile(customFilename, out error);
			if (error != null)
				Log.Fatal(string.Format("Loading file failed \"{0}\"  // {1}.", error, customFilename));


			var controlBloc = customblock.FindChild("Controls");
			if (controlBloc == null)
			{
				ResetKeyMouseSettings();
				ResetJoystickSettings();
				return;
			}
			foreach (GameControlItem item in Items)
			{

				//item.BindedKeyboardMouseValues.Clear();
				//item.bindedJoystickValues.Clear();
				var currentKeyBlock = controlBloc.FindChild(item.ControlKey.ToString());
				if (currentKeyBlock == null)
					continue;
				//keybord Setting
				var keybordBlock = currentKeyBlock.FindChild("Keybord");
				if (keybordBlock != null && keybordBlock.Children.Count > 0)
				{
					foreach (var keyBlocklock in keybordBlock.Children)
					{
						var keyboardvalue = SystemKeyboardMouseValue.Load(keyBlocklock);
						keyboardvalue.Parent = item;
						item.BindedKeyboardMouseValues.Add(keyboardvalue);
					}
				}
				//Joystick setting
				var joystickBlock = currentKeyBlock.FindChild("Joystick");
				if (joystickBlock != null && joystickBlock.Children.Count > 0)
				{
					foreach (var keyBlocklock in joystickBlock.Children)
					{
						var joystickvalue = SystemJoystickValue.Load(keyBlocklock);
						joystickvalue.Parent = item;
						item.BindedJoystickValues.Add(joystickvalue);
					}
				}

                var deadzone = currentKeyBlock.FindChild("DeadZone");
                if (deadzone != null)
                {
                    string dz = deadzone.GetAttribute("DeadZone");
                    float dzz = float.Parse(dz);
                    DeadZone = dzz;
               }
			}
		}
	}
}

