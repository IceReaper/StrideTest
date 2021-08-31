namespace StrideTest.Resources
{
	using Stride.Core.Mathematics;
	using System;
	using System.Globalization;

	public static class Serializer
	{
		public static bool TryParse<T>(string value, out T result)
		{
			if (Serializer.TryParse(typeof(T), value, out var parsed))
			{
				result = (T)parsed;

				return true;
			}

			result = default!;

			return false;
		}

		public static bool TryParse(Type type, string value, out object result)
		{
			// TODO for array of primitives, split by space and parse all.
			// TODO add dictionaries!
			if (type == typeof(bool))
				result = bool.Parse(value);
			else if (type == typeof(sbyte))
				result = sbyte.Parse(value, CultureInfo.InvariantCulture);
			else if (type == typeof(byte))
				result = byte.Parse(value, CultureInfo.InvariantCulture);
			else if (type == typeof(short))
				result = short.Parse(value, CultureInfo.InvariantCulture);
			else if (type == typeof(ushort))
				result = ushort.Parse(value, CultureInfo.InvariantCulture);
			else if (type == typeof(int))
				result = int.Parse(value, CultureInfo.InvariantCulture);
			else if (type == typeof(uint))
				result = uint.Parse(value, CultureInfo.InvariantCulture);
			else if (type == typeof(long))
				result = long.Parse(value, CultureInfo.InvariantCulture);
			else if (type == typeof(ulong))
				result = ulong.Parse(value, CultureInfo.InvariantCulture);
			else if (type == typeof(float))
				result = float.Parse(value, CultureInfo.InvariantCulture);
			else if (type == typeof(double))
				result = double.Parse(value, CultureInfo.InvariantCulture);
			else if (type == typeof(string))
				result = value;
			else if (type.IsEnum)
				result = Enum.Parse(type, value);
			else if (type == typeof(Quaternion))
				result = Serializer.ParseQuaternion(value);
			else if (type == typeof(Color))
				result = Serializer.ParseColor(value);
			else
			{
				result = default!;

				return false;
			}

			return true;
		}

		private static object ParseColor(string value)
		{
			var segments = value.Split(" ", StringSplitOptions.RemoveEmptyEntries);

			return segments.Length switch
			{
				3 => new(float.Parse(segments[0]), float.Parse(segments[1]), float.Parse(segments[2])),
				4 => new Color(float.Parse(segments[0]), float.Parse(segments[1]), float.Parse(segments[2]), float.Parse(segments[3])),
				_ => throw new FormatException("Color requires 3 or 4 parameters")
			};
		}

		private static object ParseQuaternion(string value)
		{
			var segments = value.Split(" ", StringSplitOptions.RemoveEmptyEntries);

			if (segments.Length != 3)
				throw new FormatException("Quaternion requires 3 parameters");

			return Quaternion.RotationYawPitchRoll(
				MathUtil.DegreesToRadians(float.Parse(segments[0])),
				MathUtil.DegreesToRadians(float.Parse(segments[1])),
				MathUtil.DegreesToRadians(float.Parse(segments[2]))
			);
		}
	}
}
