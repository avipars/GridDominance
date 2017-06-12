﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using MonoSAMFramework.Portable.GameMath.Geometry;
using MonoSAMFramework.Portable.Screens.ViewportAdapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// ReSharper disable ImpureMethodCallOnReadonlyValueField
namespace MonoSAMFramework.Portable.Input
{
	public class InputState
	{
		public readonly MouseState Mouse;
		public readonly KeyboardState Keyboard;
		public readonly TouchCollection TouchPanel;
		public readonly GamePadState GamePad;

		private readonly bool isDown;
		private readonly bool isJustDown;
		private readonly bool isJustUp;
		private bool isUpDownSwallowed = false;
		public InputConsumer SwallowConsumer { get; private set; } = InputConsumer.None;

		public bool IsExclusiveDown => isDown && !isUpDownSwallowed;
		public bool IsExclusiveUp => !isDown && !isUpDownSwallowed;
		public bool IsExclusiveJustDown => isJustDown && !isUpDownSwallowed;
		public bool IsExclusiveJustUp => isJustUp && !isUpDownSwallowed;
		public bool IsRealDown => isDown;
		public bool IsRealUp => !isDown;
		public bool IsRealJustDown => isJustDown;
		public bool IsRealJustUp => isJustUp;

		public void Swallow(InputConsumer ic)
		{
			isUpDownSwallowed = true;
			SwallowConsumer = ic;
		}

		public readonly FPoint HUDPointerPosition;

		public readonly FPoint   GamePointerPosition;
		public readonly FPoint   GamePointerPositionOnMap;
		public readonly FPoint[] AllGamePointerPositions;

		private readonly Dictionary<SKeys, bool> lastKeyState;
		private readonly Dictionary<SKeys, bool> currentKeyState;

		private readonly float? _pinchStartDistance = null;
		public readonly bool IsGesturePinching = false;
		public readonly bool IsGesturePinchComplete = false;
		public readonly float LastPinchPower = 0f; // squared

		private InputState(SAMViewportAdapter gameAdapter, SAMViewportAdapter hudAdapter, KeyboardState ks, MouseState ms, TouchCollection ts, GamePadState gs, InputState prev, float mox, float moy)
		{
			Mouse = ms;
			Keyboard = ks;
			TouchPanel = ts;
			GamePad = gs;


			AllGamePointerPositions = new FPoint[TouchPanel.Count];
			float sumX = 0;
			float sumY = 0;
			for (int i = 0; i < TouchPanel.Count; i++)
			{
				AllGamePointerPositions[i] = gameAdapter.PointToScreen(TouchPanel[i].Position.ToPoint());
				sumX += TouchPanel[i].Position.X;
				sumY += TouchPanel[i].Position.Y;
			}

			if (Mouse.LeftButton == ButtonState.Pressed)
			{
				isDown = true;
				GamePointerPosition = gameAdapter.PointToScreen(Mouse.Position);
				HUDPointerPosition  = hudAdapter.PointToScreen(Mouse.Position);
			}
			else if (TouchPanel.Count == 1)
			{
				isDown = true;
				GamePointerPosition = AllGamePointerPositions[0];
				HUDPointerPosition  = hudAdapter.PointToScreen(TouchPanel[0].Position.ToPoint());
			}
			else if (TouchPanel.Count == 0)
			{
				isDown = false;
				GamePointerPosition = prev.GamePointerPosition;
				HUDPointerPosition  = prev.HUDPointerPosition;
			}
			else // if (TouchPanel.Count > 1)
			{
				isDown = false; // this is correct - down onl when one finger
				GamePointerPosition = gameAdapter.PointToScreen(sumX / TouchPanel.Count, sumY / TouchPanel.Count);
				HUDPointerPosition  = hudAdapter.PointToScreen(sumX / TouchPanel.Count, sumY / TouchPanel.Count);
			}

			GamePointerPositionOnMap = GamePointerPosition.RelativeTo(mox, moy);

			isJustDown = isDown && !prev.isDown;
			isJustUp = !isDown && prev.isDown;

			lastKeyState = prev.currentKeyState;
			currentKeyState = lastKeyState.ToDictionary(p => p.Key, p => IsKeyDown(p.Key, ks, gs));


			LastPinchPower = prev.LastPinchPower;
			if (TouchPanel.Count == 2)
			{
				if (prev.IsGesturePinching)
				{
					IsGesturePinching = true;
					_pinchStartDistance = prev._pinchStartDistance;
				}

				var p1 = hudAdapter.PointToScreen(TouchPanel[0].Position.ToPoint()); // hud positions cause HUD will (?) not be resized 
				var p2 = hudAdapter.PointToScreen(TouchPanel[1].Position.ToPoint());

				while (Microsoft.Xna.Framework.Input.Touch.TouchPanel.IsGestureAvailable)
				{
					var g = Microsoft.Xna.Framework.Input.Touch.TouchPanel.ReadGesture();

					if (g.GestureType == GestureType.Pinch)
					{
						IsGesturePinching = true;
						_pinchStartDistance = prev._pinchStartDistance ?? (p1 - p2).LengthSquared();
					}
					else if (g.GestureType == GestureType.PinchComplete && prev._pinchStartDistance != null)
					{
						IsGesturePinchComplete = true;
						LastPinchPower = ((p1 - p2).LengthSquared() - prev._pinchStartDistance.Value) / 5000;
					}
				}
			}
		}

		public InputState(KeyboardState ks, MouseState ms, TouchCollection ts, GamePadState gs, float mox, float moy)
		{
			Mouse = ms;
			Keyboard = ks;
			TouchPanel = ts;
			GamePad = gs;

			isDown = false;
			GamePointerPosition = FPoint.Zero;
			HUDPointerPosition = FPoint.Zero;

			GamePointerPositionOnMap = GamePointerPosition.RelativeTo(mox, moy);

			currentKeyState = new Dictionary<SKeys, bool>(0);
		}

		private static bool IsKeyDown(SKeys key, KeyboardState ks, GamePadState gs)
		{
			switch (key)
			{
				case SKeys.ShiftAny:
					return ks.IsKeyDown(Keys.LeftShift) || ks.IsKeyDown(Keys.RightShift);
				case SKeys.ControlAny:
					return ks.IsKeyDown(Keys.LeftControl) || ks.IsKeyDown(Keys.RightControl);
				case SKeys.AltAny:
					return ks.IsKeyDown(Keys.LeftAlt) || ks.IsKeyDown(Keys.RightAlt);
				case SKeys.AndroidBack:
					return gs.Buttons.Back == ButtonState.Pressed;
				default:
					return ks.IsKeyDown((Keys) key);
			}
		}

		public static InputState GetState(SAMViewportAdapter gameAdapter, SAMViewportAdapter hudAdapter, InputState previous, float mox, float moy)
		{
			var ks = Microsoft.Xna.Framework.Input.Keyboard.GetState();
			var ms = Microsoft.Xna.Framework.Input.Mouse.GetState();
			var ts = Microsoft.Xna.Framework.Input.Touch.TouchPanel.GetState();
			var gs = Microsoft.Xna.Framework.Input.GamePad.GetState(PlayerIndex.One);

			return new InputState(gameAdapter, hudAdapter, ks, ms, ts, gs, previous, mox, moy);
		}

		public static InputState GetInitialState(float mox, float moy)
		{
			Microsoft.Xna.Framework.Input.Touch.TouchPanel.EnabledGestures = GestureType.Pinch | GestureType.PinchComplete;

			var ks = Microsoft.Xna.Framework.Input.Keyboard.GetState();
			var ms = Microsoft.Xna.Framework.Input.Mouse.GetState();
			var ts = Microsoft.Xna.Framework.Input.Touch.TouchPanel.GetState();
			var gs = Microsoft.Xna.Framework.Input.GamePad.GetState(PlayerIndex.One);

			return new InputState(ks, ms, ts, gs, mox, moy);
		}

		public bool IsKeyDown(SKeys key)
		{
			bool v;

			if (currentKeyState.TryGetValue(key, out v))
			{
				return v;
			}
			else
			{
				v = IsKeyDown(key, Keyboard, GamePad);
				currentKeyState.Add(key, v);
				return v;
			}
		}

		public bool IsKeyJustDown(SKeys key)
		{
			bool v;

			if (currentKeyState.TryGetValue(key, out v))
			{
				return v && !lastKeyState[key];
			}
			else
			{
				v = IsKeyDown(key, Keyboard, GamePad);
				currentKeyState.Add(key, v);
				lastKeyState.Add(key, v);
				return false;
			}
		}

		public bool IsModifierDown(KeyModifier mod)
		{
			bool needsCtrl  = (mod & KeyModifier.Control) == KeyModifier.Control;
			bool needsShift = (mod & KeyModifier.Shift)   == KeyModifier.Shift;
			bool needsAlt   = (mod & KeyModifier.Alt)     == KeyModifier.Alt;

			bool down = true;

			down &= needsCtrl  == IsKeyDown(SKeys.ControlAny);
			down &= needsShift == IsKeyDown(SKeys.ShiftAny);
			down &= needsAlt   == IsKeyDown(SKeys.AltAny);
			
			return down;
		}

		public bool IsShortcutJustPressed(KeyModifier mod, SKeys key)
		{
			return IsModifierDown(mod) && IsKeyJustDown(key);
		}

		public bool IsShortcutJustPressed(KeyCombination key)
		{
			return IsModifierDown(key.Mod) && IsKeyJustDown(key.Key);
		}

		public bool IsShortcutPressed(KeyModifier mod, SKeys key)
		{
			return IsModifierDown(mod) && IsKeyDown(key);
		}

		public bool IsShortcutPressed(KeyCombination key)
		{
			return IsModifierDown(key.Mod) && IsKeyDown(key.Key);
		}

		public string GetFullDebugSummary()
		{
			StringBuilder sb = new StringBuilder();
			foreach (Keys k in Enum.GetValues(typeof(Keys)))
			{
				if (IsKeyDown((SKeys)k, Keyboard, GamePad)) sb.AppendLine("[X] Keys." + k);
			}
			foreach (SKeys k in Enum.GetValues(typeof(SKeys)))
			{
				if (IsKeyDown(k, Keyboard, GamePad)) sb.AppendLine("[X] SKeys." + k);
			}

			foreach (Buttons b in Enum.GetValues(typeof(Buttons)))
			{
				if (GamePad.IsButtonDown(b)) sb.AppendLine("[X] Buttons." + b);
			}

			var s = sb.ToString().Trim();

			return s == "" ? "[None]" : s;

		}

		public char? GetCharJustDown()
		{
			var shift = IsKeyDown(SKeys.ShiftAny);

			if (IsKeyJustDown(SKeys.Space)) return ' ';

			for (SKeys chr = SKeys.A; chr < SKeys.Z; chr++)
			{
				if (IsKeyJustDown(chr)) return (char)(chr - SKeys.A + (shift ? 'A' : 'a'));
			}

			if (IsKeyJustDown(SKeys.D0)) return '0';
			if (IsKeyJustDown(SKeys.D1)) return '1';
			if (IsKeyJustDown(SKeys.D2)) return '2';
			if (IsKeyJustDown(SKeys.D3)) return '3';
			if (IsKeyJustDown(SKeys.D4)) return '4';
			if (IsKeyJustDown(SKeys.D5)) return '5';
			if (IsKeyJustDown(SKeys.D6)) return '6';
			if (IsKeyJustDown(SKeys.D7)) return '7';
			if (IsKeyJustDown(SKeys.D8)) return '8';
			if (IsKeyJustDown(SKeys.D9)) return '9';

			return null;
		}
	}
}
