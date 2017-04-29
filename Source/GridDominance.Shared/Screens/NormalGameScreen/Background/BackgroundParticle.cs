﻿using System.Collections.Generic;
using GridDominance.Shared.Screens.NormalGameScreen.Fractions;
using MonoSAMFramework.Portable.GameMath.Geometry.Alignment;

namespace GridDominance.Shared.Screens.NormalGameScreen.Background
{
	class BackgroundParticle
	{
		public const float PARTICLE_SPEED = 100;

		public const int INITIAL_POWER = 16;

		public bool Alive = true;

		public readonly Fraction Fraction;
		public int RemainingPower;

		public float PowerPercentage => RemainingPower*1f/INITIAL_POWER;

		public readonly FlatAlign4 Direction;

		public readonly int OriginX;
		public readonly int OriginY;

		public float X;
		public float Y;

		public List<BackgroundParticle> TravelSection;

		public BackgroundParticle(int ox, int oy, Fraction f, float px, float py, FlatAlign4 dir)
		{
			OriginX = ox;
			OriginY = oy;

			X = px;
			Y = py;

			Fraction = f;
			RemainingPower = INITIAL_POWER;
			Direction = dir;
		}

		public BackgroundParticle(BackgroundParticle source, float px, float py, FlatAlign4 dir)
		{
			OriginX = source.OriginX;
			OriginY = source.OriginY;

			X = px;
			Y = py;

			Fraction = source.Fraction;
			RemainingPower = source.RemainingPower - 1;
			Direction = dir;
		}
	}
}