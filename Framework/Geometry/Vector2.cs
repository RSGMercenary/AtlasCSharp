﻿using System;

namespace Atlas.Framework.Geometry
{
	/*class Vector2
	{

	}*/


	class Vector2:IVector2<Vector2>
	{
		public static readonly IReadOnlyVector2 Up = new Vector2(0, 1);
		public static readonly IReadOnlyVector2 Down = new Vector2(0, -1);
		public static readonly IReadOnlyVector2 Left = new Vector2(-1, 0);
		public static readonly IReadOnlyVector2 Right = new Vector2(0, 1);

		private float x = 0;
		private float y = 0;

		public Vector2()
		{

		}

		public Vector2(float x = 0, float y = 0)
		{
			Set2(x, y);
		}

		public Vector2(IReadOnlyVector2 vector)
		{
			Set2(vector);
		}

		override public string ToString()
		{
			return "(" + X + ", " + Y + ")";
		}

		#region Properties

		public virtual float X
		{
			get { return x; }
			set { x = value; }
		}

		public virtual float Y
		{
			get { return y; }
			set { y = value; }
		}

		public float LengthSquared2
		{
			get { return X * X + Y * Y; }
		}

		public float Length2
		{
			get { return (float)Math.Sqrt(LengthSquared2); }
		}

		public float RadiansZ
		{
			get { return (float)Math.Atan2(y, x); }
		}

		public float DegreesZ
		{
			get { return RadiansZ * (float)(180 / Math.PI); }
		}

		#endregion

		#region Vector

		public Vector2 Set(float value)
		{
			return Set2(value, value);
		}

		public Vector2 Add(float value)
		{
			return Add2(value, value);
		}

		public Vector2 Subtract(float value)
		{
			return Subtract2(value, value);
		}

		public Vector2 Multiply(float value)
		{
			return Multiply2(value, value);
		}

		public Vector2 Divide(float value)
		{
			return Divide2(value, value);
		}

		#endregion

		#region Vector2 Float Params

		public Vector2 Set2(float x, float y)
		{
			X = x;
			Y = y;
			return this;
		}

		public Vector2 Add2(float x = 0, float y = 0)
		{
			X += x;
			Y += y;
			return this;
		}

		public Vector2 Subtract2(float x = 0, float y = 0)
		{
			X -= x;
			Y -= y;
			return this;
		}

		public Vector2 Multiply2(float x = 1, float y = 1)
		{
			X *= x;
			Y *= y;
			return this;
		}

		public Vector2 Divide2(float x = 1, float y = 1)
		{
			X /= x;
			Y /= y;
			return this;
		}

		public Vector2 Reflect2(float x, float y)
		{
			float dot = 2 * Dot2(x, y);
			X -= dot * x;
			Y -= dot * y;
			return this;
		}

		public float Dot2(float x, float y)
		{
			return X * x + Y * y;
		}

		public float Cross2(float x, float y)
		{
			return X * x - Y * y;
		}

		public Vector2 Normalize2(float length = 1)
		{
			float ratio = Math.Abs(length) / Length2;
			return Multiply2(ratio, ratio);
		}

		public Vector2 PerpendicularCCW2()
		{
			return Set2(Y, -X);
		}

		public Vector2 PerpendicularCW2()
		{
			return Set2(-Y, X);
		}

		#endregion

		#region Vector2 IReadOnlyVector2 Params

		public Vector2 Set2(IReadOnlyVector2 vector)
		{
			return Set2(vector.X, vector.Y);
		}

		public Vector2 Add2(IReadOnlyVector2 vector)
		{
			return Add2(vector.X, vector.Y);
		}

		public Vector2 Subtract2(IReadOnlyVector2 vector)
		{
			return Subtract2(vector.X, vector.Y);
		}

		public Vector2 Multiply2(IReadOnlyVector2 vector)
		{
			return Multiply2(vector.X, vector.Y);
		}

		public Vector2 Divide2(IReadOnlyVector2 vector)
		{
			return Divide2(vector.X, vector.Y);
		}

		public float Dot2(IReadOnlyVector2 vector)
		{
			return Dot2(vector.X, vector.Y);
		}

		public float Cross2(IReadOnlyVector2 vector)
		{
			return Cross2(vector.X, vector.Y);
		}

		public Vector2 Reflect2(IReadOnlyVector2 vector)
		{
			Vector2 normal = new Vector2(vector).Normalize2();
			return Reflect2(normal.X, normal.Y);
		}

		#endregion
	}
}
