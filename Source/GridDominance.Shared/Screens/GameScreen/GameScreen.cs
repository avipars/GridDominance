﻿using FarseerPhysics;
using GridDominance.Shared.Framework;
using GridDominance.Shared.Framework.DebugDisplay;
using GridDominance.Shared.Screens.GameScreen.Background;
using GridDominance.Shared.Screens.GameScreen.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.InputListeners;
using FarseerPhysics.DebugView;
using FarseerPhysics.Dynamics;
using GridDominance.Levelformat.Parser;
using GridDominance.Shared.Resources;
using System.Collections.Generic;
using System.Linq;

namespace GridDominance.Shared.Screens.GameScreen
{
	class GameScreen : GDScreen
	{
		public const int TILE_WIDTH = 64;

		public const int VIEW_WIDTH  = 16 * TILE_WIDTH; // 1024
		public const int VIEW_HEIGHT = 10 * TILE_WIDTH; // 640

		//-----------------------------------------------------------------

#if DEBUG
		private RealtimeAPSCounter fpsCounter;
		private RealtimeAPSCounter upsCounter;
#endif

		public TolerantBoxingViewportAdapter Viewport;
		private GDEntityManager entities;
		private IDebugTextDisplay debugDisp;

		private InputStateManager inputStateMan;
		private InputListenerManager inputs;
		private MouseListener mouseListener;
		private TouchListener touchListener;

		private SpriteBatch mainBatch;

		private GameGridBackground background;

		private Fraction fractionNeutral;
		private Fraction fractionPlayer;
		private Fraction fractionComputer1;
		private Fraction fractionComputer2;
		private Fraction fractionComputer3;

		private readonly LevelFile blueprint;

		public GameScreen(MainGame game, GraphicsDeviceManager gdm, LevelFile bp) 
			: base(game, gdm)
		{
			blueprint = bp;

			Initialize();
		}

		private void Initialize()
		{
			mainBatch = new SpriteBatch(Graphics.GraphicsDevice);
			Viewport = new TolerantBoxingViewportAdapter(Owner.Window, Graphics, VIEW_WIDTH, VIEW_HEIGHT);
			inputs = new InputListenerManager(Viewport);
			inputStateMan = new InputStateManager(Viewport);
			background = new GameGridBackground(Graphics.GraphicsDevice, Viewport);
			entities = new GDEntityManager(this);

			ConvertUnits.SetDisplayUnitToSimUnitRatio(GDSettings.PHYSICS_CONVERSION_FACTOR);

			mouseListener = inputs.AddListener(new MouseListenerSettings());
			touchListener = inputs.AddListener(new TouchListenerSettings());


#if DEBUG
			fpsCounter = new RealtimeAPSCounter();
			upsCounter = new RealtimeAPSCounter();

			debugDisp = new DebugTextDisplay(Graphics.GraphicsDevice);
			{
				debugDisp.AddLine(() => $"FPS = {fpsCounter.AverageAPS:0000.0} (current = {fpsCounter.CurrentAPS:0000.0} | delta = {fpsCounter.AverageDelta*1000:000.00} | min = {fpsCounter.MinimumAPS:0000.0} | total = {fpsCounter.TotalActions:000000})");
				debugDisp.AddLine(() => $"UPS = {upsCounter.AverageAPS:0000.0} (current = {upsCounter.CurrentAPS:0000.0} | delta = {upsCounter.AverageDelta*1000:000.00} | min = {upsCounter.MinimumAPS:0000.0} | total = {upsCounter.TotalActions:000000})");
				debugDisp.AddLine(() => $"Entities = {entities.Count()}");
				debugDisp.AddLine(() => $"Pointer = ({inputStateMan.GetCurrentState().PointerPosition.X:000.0}|{inputStateMan.GetCurrentState().PointerPosition.Y:000.0})");
			}

			mouseListener.MouseDown += (o, a) => debugDisp.AddDecayLine($"Mouse::OnDown({a.Position.X:0000}|{a.Position.Y:0000})", 0.75f, 0.5f, 0.25f);
			mouseListener.MouseUp += (o, a) => debugDisp.AddDecayLine($"Mouse::OnUp({a.Position.X:0000}|{a.Position.Y:0000})", 0.75f, 0.5f, 0.25f);
			touchListener.TouchStarted += (o, a) => debugDisp.AddDecayLine($"TouchPad::OnDown({a.Location.Position.X:0000}|{a.Location.Position.Y:0000})", 0.75f, 0.5f, 0.25f);
			touchListener.TouchEnded += (o, a) => debugDisp.AddDecayLine($"TouchPad::OnUp({a.Location.Position.X:0000}|{a.Location.Position.Y:0000})", 0.75f, 0.5f, 0.25f);
#else
			debugDisp = new DummyDebugTextDisplay();
#endif


			//--------------------

			fractionNeutral   = Fraction.CreateNeutralFraction();
			fractionPlayer    = Fraction.CreatePlayerFraction(fractionNeutral);
			fractionComputer1 = Fraction.CreateComputerFraction(Fraction.COLOR_COMPUTER_01, fractionNeutral, Fraction.MULTIPLICATOR_COMPUTER_0);
			fractionComputer2 = Fraction.CreateComputerFraction(Fraction.COLOR_COMPUTER_02, fractionNeutral, Fraction.MULTIPLICATOR_COMPUTER_0);
			fractionComputer3 = Fraction.CreateComputerFraction(Fraction.COLOR_COMPUTER_03, fractionNeutral, Fraction.MULTIPLICATOR_COMPUTER_0);

			LoadLevelFromBlueprint();
		}

		private void LoadLevelFromBlueprint()
		{
			Fraction[] fracList = 
			{
				fractionNeutral,
				fractionPlayer,
				fractionComputer1,
				fractionComputer2,
				fractionComputer3,
			};

			foreach (var bPrint in blueprint.BlueprintCannons)
			{
				entities.AddEntity(new Cannon(this, bPrint, fracList));
			}
		}

		public override void Update(GameTime gameTime)
		{
#if DEBUG
			upsCounter.Update(gameTime);
#endif

			var state = inputStateMan.GetNewState();
			
			if (state.IsExit()) Owner.Exit();
			
			inputs.Update(gameTime);

			entities.Update(gameTime, state);

			debugDisp.Update(gameTime, state);
		}

		public override void Draw(GameTime gameTime)
		{
#if DEBUG
			fpsCounter.Update(gameTime);
#endif
			
			Graphics.GraphicsDevice.Clear(Color.OrangeRed);
			
			mainBatch.Begin(transformMatrix: Viewport.GetScaleMatrix());
			{
				background.Draw(mainBatch);
			
				entities.Draw(mainBatch);
			}
			mainBatch.End();

			entities.DrawRest();

			debugDisp.Draw(gameTime);
		}

		public void PushNotification(string text)
		{
			debugDisp.AddDecayLine(text);
		}

		public IEnumerable<T> GetEntities<T>()
		{
			return entities.Enumerate().OfType<T>();
		}

		public World GetPhysicsWorld()
		{
			return entities.PhysicsWorld;
		}
	}
}