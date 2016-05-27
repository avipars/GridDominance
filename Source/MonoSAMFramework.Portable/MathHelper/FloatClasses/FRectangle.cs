﻿using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace MonoSAMFramework.Portable.MathHelper.FloatClasses
{
	[DataContract]
	[DebuggerDisplay("{DebugDisplayString,nq}")]
	public struct FRectangle : IEquatable<FRectangle>
	{
		public const float EPSILON = 1E-10f;

		public static readonly FRectangle Empty = new FRectangle(0, 0, 0, 0);

		[DataMember]
		public readonly float X;
		[DataMember]
		public readonly float Y;
		[DataMember]
		public readonly float Width;
		[DataMember]
		public readonly float Height;

		public FRectangle(float x, float y, float width, float height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

		public FRectangle(FPoint location, FPoint size)
		{
			X = location.X;
			Y = location.Y;
			Width = size.X;
			Height = size.Y;
		}

		public FRectangle(FPoint location, FSize size)
		{
			X = location.X;
			Y = location.Y;
			Width = size.Width;
			Height = size.Height;
		}

		public static bool operator ==(FRectangle a, FRectangle b)
		{
			return
				(Math.Abs(a.X - b.X) < EPSILON) &&
				(Math.Abs(a.Y - b.Y) < EPSILON) &&
				(Math.Abs(a.Width - b.Width) < EPSILON) &&
				(Math.Abs(a.Height - b.Height) < EPSILON);
		}

		public static bool operator !=(FRectangle a, FRectangle b)
		{
			return !(a == b);
		}
		
		public bool Contains(float x, float y)
		{
			return 
				x >= X && 
				y >= Y && 
				x < (X + Width) && 
				y < (Y + Height);
		}

		public bool Contains(FPoint p)
		{
			return 
				p.X >= X && 
				p.Y >= Y && 
				p.X < (X + Width) && 
				p.Y < (Y + Height);
		}
		
		public bool Contains(Vector2 v)
		{
			return 
				v.X >= X && 
				v.Y >= Y && 
				v.X < (X + Width) && 
				v.Y < (Y + Height);
		}

		public bool Contains(Rectangle value)
		{
			return 
				X <= value.X && 
				Y <= value.Y && 
				value.X + value.Width <= X + Width && 
				value.Y + value.Height <= Y + Height;
		}

		public override bool Equals(object obj)
		{
			return obj is FRectangle && this == (FRectangle)obj;
		}

		public bool Equals(FRectangle other)
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode() ^ Width.GetHashCode() ^ Height.GetHashCode();
		}

		public bool Intersects(FRectangle value)
		{
			return value.Left < Right && Left < value.Right && value.Top < Bottom && Top < value.Bottom;
		}

		public override string ToString()
		{
			return $"{{X:{X} Y:{Y} Width:{Width} Height:{Height}}}";
		}

		internal string DebugDisplayString => $"({X}|{Y}):({Width}|{Height})";

		public FRectangle Union(FRectangle value2)
		{
			float num = FloatMath.Min(X, value2.X);
			float num2 = FloatMath.Min(Y, value2.Y);
			return new FRectangle(num, num2, System.Math.Max(Right, value2.Right) - num, System.Math.Max(Bottom, value2.Bottom) - num2);
		}

		public FRectangle AsInflated(int horizontalAmount, int verticalAmount)
		{
			return new FRectangle(
				X + horizontalAmount,
				Y + verticalAmount,
				Width - horizontalAmount * 2,
				Height - verticalAmount * 2);
		}

		public FRectangle AsInflated(float horizontalAmount, float verticalAmount)
		{
			return new FRectangle(
				X + horizontalAmount,
				Y + verticalAmount,
				Width - horizontalAmount * 2,
				Height - verticalAmount * 2);
		}

		public FRectangle AsOffseted(float offsetX, float offsetY)
		{
			return new FRectangle(
				X + offsetX,
				Y + offsetY,
				Width,
				Height);
		}

		public FRectangle AsOffseted(Vector2 offset)
		{
			return new FRectangle(
				X + offset.X,
				Y + offset.Y,
				Width,
				Height);
		}

		public FRectangle AsResized(FSize size)
		{
			return new FRectangle(
				X + Width / 2 - size.Width / 2,
				Y + Height / 2 - size.Height / 2,
				size.Width,
				size.Height);
		}

		public float Left   => X;
		public float Right  => X + Width;
		public float Top    => Y;
		public float Bottom => Y + Height;

		public bool IsEmpty => Math.Abs(Width) < EPSILON && Math.Abs(Height) < EPSILON && Math.Abs(X) < EPSILON && Math.Abs(Y) < EPSILON;

		public FPoint TopLeft            => new FPoint(Left, Top);
		public FPoint TopRight           => new FPoint(Right, Top);
		public FPoint BottomLeft         => new FPoint(Left, Bottom);
		public FPoint BottomRight        => new FPoint(Right, Bottom);

		public Vector2 VectorTopLeft     => new Vector2(Left, Top);
		public Vector2 VectorTopRight    => new Vector2(Right, Top);
		public Vector2 VectorBottomLeft  => new Vector2(Left, Bottom);
		public Vector2 VectorBottomRight => new Vector2(Right, Bottom);

		public FPoint Location => new FPoint(X, Y);
		public FSize Size => new FSize(Width, Height);
		public FPoint Center => new FPoint(X + Width / 2, Y + Height / 2);

		public Rectangle Truncate()
		{
			return new Rectangle((int)X, (int)Y, (int)Width, (int)Height);
		}

		public Rectangle Round()
		{
			return new Rectangle(FloatMath.Round(X), FloatMath.Round(Y), FloatMath.Round(Width), FloatMath.Round(Height));
		}
	}
}