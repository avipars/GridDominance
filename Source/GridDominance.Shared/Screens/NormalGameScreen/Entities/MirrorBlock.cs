﻿using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using GridDominance.Levelfileformat.Blueprint;
using GridDominance.Shared.Resources;
using GridDominance.Shared.Screens.ScreenGame;
using Microsoft.Xna.Framework;
using MonoSAMFramework.Portable.BatchRenderer;
using MonoSAMFramework.Portable.GameMath.Geometry;
using MonoSAMFramework.Portable.Input;
using MonoSAMFramework.Portable.Screens;
using MonoSAMFramework.Portable.Screens.Entities;

namespace GridDominance.Shared.Screens.NormalGameScreen.Entities
{
	public class MirrorBlock : GameEntity
	{
		public override Vector2 Position { get; }
		public override FSize DrawingBoundingBox { get; }
		public override Color DebugIdentColor { get; } = Color.Transparent;

		private readonly float _width;
		private readonly float _height;

		private readonly FRectangle _bounds;

		public Body PhysicsBody;
		public Fixture PhysicsFixture;

		public MirrorBlock(GDGameScreen scrn, MirrorBlockBlueprint blueprint) : base(scrn, GDConstants.ORDER_GAME_WALL)
		{
			var pos = new Vector2(blueprint.X, blueprint.Y);

			_width = blueprint.Width;
			_height = blueprint.Height;

			_bounds = FRectangle.CreateByCenter(pos, _width, _height);

			Position = pos;

			DrawingBoundingBox = new FSize(_width, _height);

			this.GDOwner().GDBackground.RegisterBlockedBlock(_bounds);
		}

		public override void OnInitialize(EntityManager manager)
		{
			PhysicsBody = BodyFactory.CreateBody(this.GDManager().PhysicsWorld, ConvertUnits.ToSimUnits(Position), 0, BodyType.Static);

			PhysicsFixture = FixtureFactory.AttachRectangle(ConvertUnits.ToSimUnits(_width), ConvertUnits.ToSimUnits(_height), 1, Vector2.Zero, PhysicsBody, this);
		}
		
		public override void OnRemove()
		{
			//
		}

		protected override void OnUpdate(SAMTime gameTime, InputState istate)
		{
			//
		}

		protected override void OnDraw(IBatchRenderer sbatch)
		{
			//TODO
		}
	}
}
