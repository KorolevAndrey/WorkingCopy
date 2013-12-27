// Copyright (C) 2006-2010 NeoAxis Group Ltd.

using System;
using Engine;
using Engine.UISystem;
using ProjectCommon;

namespace Game
{
	public class KeyListener : Control
	{
		GameControlsManager.GameControlItem controlItem;
		private GameControlsManager.SystemKeyboardMouseValue _newKeyboardMousevalue;
		private GameControlsManager.SystemJoystickValue _newJoystickValue;

		private GameControlsManager.SystemKeyboardMouseValue _oldKeyboardMouseValue;
		private GameControlsManager.SystemJoystickValue _oldJoystickValue;

		private GameControlsManager.SystemKeyboardMouseValue _conflictKeyboardMouseValue;
		private GameControlsManager.SystemJoystickValue _conflictJoystickValue;

		public KeyListener( object sender )
		{
			var list = sender as ListBox;

			var keybordvalue = list.SelectedItem as GameControlsManager.SystemKeyboardMouseValue;
			if( keybordvalue != null )
			{
				_oldKeyboardMouseValue = keybordvalue;
				controlItem = keybordvalue.Parent;
			}
			var joystickvalue = list.SelectedItem as GameControlsManager.SystemJoystickValue;
			if( joystickvalue != null )
			{
				_oldJoystickValue = joystickvalue;
				controlItem = joystickvalue.Parent;
			}
		}

		protected override bool OnKeyDown( KeyEvent e )
		{
			if( base.OnKeyDown( e ) )
				return true;
			if( _newJoystickValue != null || _newKeyboardMousevalue != null )
				return true;
			if( e.Key == EKeys.Escape )
			{
				SetShouldDetach();
				return true;
			}
			if( controlItem != null && _oldKeyboardMouseValue != null )
			{
				_newKeyboardMousevalue = new GameControlsManager.SystemKeyboardMouseValue( e.Key ) { Parent = controlItem };
				GameControlsManager.SystemKeyboardMouseValue key;
				if( GameControlsManager.Instance.IsAlreadyBinded( e.Key, out key ) )
				{
					_conflictKeyboardMouseValue = key;
					CreateConfirmDialogue( "Key " + e.Key + " is already bound to " + key.Parent.ControlKey + ". Override ?, or Click Clear to remove the bind" );

					return true;
				}
				SetKey();
				SetShouldDetach();
				return true;
			}

			return false;
		}

		public bool DoMouseWheel( int delta )
		{
			return OnMouseWheel( delta );
		}
		protected override bool OnMouseWheel( int delta )
		{
			if( base.OnMouseWheel( delta ) )
				return true;
			if( _newJoystickValue != null || _newKeyboardMousevalue != null )
				return true;
			if( controlItem != null && _oldKeyboardMouseValue != null )
			{
				var scrollDirection = delta > 0 ? MouseScroll.ScrollUp : MouseScroll.ScrollDown;
				_newKeyboardMousevalue = new GameControlsManager.SystemKeyboardMouseValue( scrollDirection ) { Parent = controlItem };
				GameControlsManager.SystemKeyboardMouseValue key;
				if( GameControlsManager.Instance.IsAlreadyBinded( scrollDirection, out key ) )
				{
					_conflictKeyboardMouseValue = key;
					CreateConfirmDialogue( "Mouse scroll " + scrollDirection + " is already bound to " + key.Parent.ControlKey + ". Override? or Click Clear to remove the bind" );
					return true;
				}
				SetKey();
				SetShouldDetach();
				return true;
			}
			return false;
		}

		protected override bool OnMouseDown( EMouseButtons button )
		{
			if( base.OnMouseDown( button ) )
				return true;
			if( _newJoystickValue != null || _newKeyboardMousevalue != null )
				return true;
			if( controlItem != null && _oldKeyboardMouseValue != null )
			{
				_newKeyboardMousevalue = new GameControlsManager.SystemKeyboardMouseValue( button ) { Parent = controlItem };
				GameControlsManager.SystemKeyboardMouseValue key;
				if( GameControlsManager.Instance.IsAlreadyBinded( button, out key ) )
				{
					_conflictKeyboardMouseValue = key;
					CreateConfirmDialogue( "Mouse button " + button + " is already bound to " + key.Parent.ControlKey + ". Override ? or Click Clear to remove the bind" );
					return true;
				}
				SetKey();
				SetShouldDetach();
				return true;
			}

			return false;
		}

		void SetKey()
		{
			if( _newKeyboardMousevalue != null && _oldKeyboardMouseValue != null )
			{
				if( !_oldKeyboardMouseValue.Unbound )
					controlItem.BindedKeyboardMouseValues.Remove( _oldKeyboardMouseValue );
				controlItem.BindedKeyboardMouseValues.Add( _newKeyboardMousevalue );
			}
			if( _newJoystickValue != null && _oldJoystickValue != null )
			{
				if( !_oldJoystickValue.Unbound )
					controlItem.BindedJoystickValues.Remove( _oldJoystickValue );
				controlItem.BindedJoystickValues.Add( _newJoystickValue );
			}
		}

		//bit hacky but it works
		void ClearKey()
		{
			if( _newKeyboardMousevalue != null && _oldKeyboardMouseValue != null )
			{
				if( !_oldKeyboardMouseValue.Unbound )
					controlItem.BindedKeyboardMouseValues.Remove( _oldKeyboardMouseValue );
				//controlItem.BindedKeyboardMouseValues.Add(_newKeyboardMousevalue);
			}
			if( _newJoystickValue != null && _oldJoystickValue != null )
			{
				if( !_oldJoystickValue.Unbound )
					controlItem.BindedJoystickValues.Remove( _oldJoystickValue );
				//controlItem.BindedJoystickValues.Add(_newJoystickValue);
			}
			GameControlsManager.Instance.SaveCustomConfig();
		}

		void RemoveDuplicate()
		{
			if( _conflictKeyboardMouseValue != null )
			{
				_conflictKeyboardMouseValue.Parent.BindedKeyboardMouseValues.Remove( _conflictKeyboardMouseValue );
			}
			if( _conflictJoystickValue != null )
			{
				_conflictJoystickValue.Parent.BindedJoystickValues.Remove( _conflictJoystickValue );
			}
		}

		private void CancelButton_Click( object sender )
		{
			SetShouldDetach();
		}

		private void OKButton_Click( object sender )
		{
			RemoveDuplicate();
			SetKey();
			SetShouldDetach();

		}

		private void ClearButton_Click( object sender )
		{
			ClearKey();
			SetShouldDetach();
		}

		protected override bool OnJoystickEvent( JoystickInputEvent e )
		{

			if( base.OnJoystickEvent( e ) )
				return true;
			if( _newJoystickValue != null || _newKeyboardMousevalue != null )
				return true;
			if( controlItem != null && _oldJoystickValue != null )
			{
				string message = "";
				//JoystickButtonDownEvent
				{
					var evt = e as JoystickButtonDownEvent;
					if( evt != null )
					{
						_newJoystickValue = new GameControlsManager.SystemJoystickValue( evt.Button.Name ) { Parent = controlItem };
						GameControlsManager.SystemJoystickValue key;
						if( GameControlsManager.Instance.IsAlreadyBinded( evt.Button.Name, out key ) )
						{
							message = "Button " + evt.Button.Name + " is already bound to  " + key.Parent.ControlKey + ". Override? or Click Clear to remove the bind";
							_conflictJoystickValue = key;
						}
					}
				}
				//JoystickAxisChangedEvent
				{
					var evt = e as JoystickAxisChangedEvent;
					if( evt != null )
					{
						var filter = JoystickAxisFilters.DEADZONE;
						if( evt.Axis.Value < -GameControlsManager.Instance.DeadZone )
						{
							filter = JoystickAxisFilters.LessZero;
						}
						else if( evt.Axis.Value > GameControlsManager.Instance.DeadZone )
						{
							filter = JoystickAxisFilters.GreaterZero;
						}

						//pass the dead zone
						if( filter != JoystickAxisFilters.DEADZONE )
						{
							_newJoystickValue = new GameControlsManager.SystemJoystickValue( evt.Axis.Name, filter ) { Parent = controlItem };
							GameControlsManager.SystemJoystickValue key;
							if( GameControlsManager.Instance.IsAlreadyBinded( evt.Axis.Name, filter, out key ) )
							{
								message = "Axis " + evt.Axis.Name + "(" + filter + ") is already bound to  " + key.Parent.ControlKey + ". Override ?, or Click Clear to remove the bind";
								_conflictJoystickValue = key;
							}
						}
					}
				}

				{
					var evt = e as JoystickPOVChangedEvent;
					if( evt != null )
					{

						_newJoystickValue = new GameControlsManager.SystemJoystickValue( evt.POV.Name, evt.POV.Value ) { Parent = controlItem };
						GameControlsManager.SystemJoystickValue key;
						if( GameControlsManager.Instance.IsAlreadyBinded( evt.POV.Name, evt.POV.Value, out key ) )
						{
							message = "Pov " + evt.POV.Name + "(" + evt.POV.Value + ") is already bound to  " + key.Parent.ControlKey + ". Override? or Click Clear to remove the bind ";
							_conflictJoystickValue = key;
						}
					}
				}

				{
					var evt = e as JoystickSliderChangedEvent;
					if( evt != null )
					{
						//    var currentValue = evt.Axe == JoystickSliderAxes.X
						//                ? evt.Slider.Value.X
						//                : evt.Slider.Value.Y;

						//    var filter = JoystickAxisFilters.DEADZONE;
						//    if (currentValue < -GameControlsManager.DeadZone)
						//    {
						//        filter = JoystickAxisFilters.LessZero;
						//    }
						//    else if (currentValue > GameControlsManager.DeadZone)
						//    {
						//        filter = JoystickAxisFilters.GreaterZero;
						//    }

						//    //pass the dead zone
						//    if (filter != JoystickAxisFilters.DEADZONE)
						//    {
						//        _newJoystickValue = new GameControlsManager.SystemJoystickValue(evt.Slider.Name, evt.Axe,filter)
						//        {
						//            Parent = controlItem
						//        };
						//        GameControlsManager.SystemJoystickValue key;
						//        if (GameControlsManager.Instance.IsAlreadyBinded(evt.Slider.Name, evt.Axe, out key))
						//        {
						//            message = "Slider " + evt.Slider.Name + "(" + evt.Axe + ") is already bound to  " +
						//                      key.Parent.ControlKey + ". Override ?";
						//            _conflictJoystickValue = key;
						//        }
						//    }
					}
				}

				if( _conflictJoystickValue != null )
				{
					CreateConfirmDialogue( message );
					return true;
				}
				SetKey();
				SetShouldDetach();
				return true;
			}
			return false;
		}

		void CreateConfirmDialogue( string message )
		{
			Control confirmControl = ControlDeclarationManager.Instance.CreateControl( @"GUI\Confirm.gui" );
			Controls.Add( confirmControl );
			confirmControl.Controls[ "MessageBox" ].Text = message;
			MouseCover = true;
			( (Button)confirmControl.Controls[ "Cancel" ] ).Click += CancelButton_Click;
			( (Button)confirmControl.Controls[ "OK" ] ).Click += OKButton_Click;
			( (Button)confirmControl.Controls[ "Clear" ] ).Click += ClearButton_Click;
		}
	}
}