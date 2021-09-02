namespace StrideTest.Assets.Yaml
{
	using Stride.Core.Mathematics;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;

	public static class YamlSerializer
	{
		public static bool TryDeserialize<T>(YamlNode value, out T? result)
		{
			if (YamlSerializer.TryDeserializeComplex(typeof(T), value, out var parsed) && parsed != null)
			{
				result = (T)parsed;

				return true;
			}

			result = default;

			return false;
		}

		public static void Deserialize(object context, YamlNode values)
		{
			foreach (var (fieldName, value) in values)
			{
				var field = context.GetType().GetField(fieldName);

				if (field == null)
				{
					Console.WriteLine($"Unable to find field '{fieldName}' on type '{context.GetType().Name}'");

					continue;
				}

				if (YamlSerializer.TryDeserializeComplex(field.FieldType, value, out var fieldValue))
					field.SetValue(context, fieldValue);
			}
		}

		private static bool TryDeserializeComplex(Type type, YamlNode value, out object? result)
		{
			var useType = Nullable.GetUnderlyingType(type) ?? type;

			if (useType.IsAssignableTo(typeof(IDictionary)))
			{
				var arguments = useType.GetGenericArguments();

				if (Activator.CreateInstance(useType) is not IDictionary dictionary)
				{
					Console.WriteLine($"Unable to build dictionary for key type {arguments[0].Name} and value type {arguments[1].Name}");
					result = null;
				}
				else
				{
					foreach (var (keyString, valueYaml) in value)
					{
						if (YamlSerializer.TryDeserializeSimple(arguments[0], keyString, out var dictKey)
							&& dictKey != null
							&& YamlSerializer.TryDeserializeComplex(arguments[1], valueYaml, out var dictValue))
							dictionary.Add(dictKey, dictValue);
						else
							Console.WriteLine($"Unable to add entry {keyString} to dictionary");
					}

					result = dictionary;
				}
				
				return result != null;
			}

			var valueChunks = value.Value == null ? new[] { "" } : YamlSerializer.SplitValue(value.Value);

			if (YamlSerializer.TryDeserializeSimple(useType, valueChunks[0], out result, true))
			{
				if (valueChunks.Length > 1)
					Console.WriteLine($"Ignoring primitive values '2-{valueChunks.Length}' for type {useType.Name}");

				return true;
			}

			if (useType.IsArray)
			{
				var results = new List<object>();

				foreach (var valueChunk in valueChunks)
				{
					if (YamlSerializer.TryDeserializeSimple(useType.GetElementType()!, valueChunk, out var arrayValueParsed) && arrayValueParsed != null)
						results.Add(arrayValueParsed);
				}

				result = results.ToArray();

				return true;
			}

			if (useType.IsClass)
			{
				result = Activator.CreateInstance(useType);

				if (result == null)
				{
					Console.WriteLine($"Unable to create new instance of type '{useType.Name}'");

					return false;
				}

				YamlSerializer.Deserialize(result, value);

				return true;
			}

			Console.WriteLine($"Unable to parse type '{useType.Name}'");

			return false;
		}

		private static bool TryDeserializeSimple(Type type, string value, out object? result, bool silent = false)
		{
			result = null;

			if (type == typeof(bool) && bool.TryParse(value, out var boolResult))
				result = boolResult;
			else if (type == typeof(sbyte) && sbyte.TryParse(value, out var sbyteResult))
				result = sbyteResult;
			else if (type == typeof(byte) && byte.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var byteResult))
				result = byteResult;
			else if (type == typeof(short) && short.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var shortResult))
				result = shortResult;
			else if (type == typeof(ushort) && ushort.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ushortResult))
				result = ushortResult;
			else if (type == typeof(int) && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intResult))
				result = intResult;
			else if (type == typeof(uint) && uint.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var uintResult))
				result = uintResult;
			else if (type == typeof(long) && long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var longResult))
				result = longResult;
			else if (type == typeof(ulong) && ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ulongResult))
				result = ulongResult;
			else if (type == typeof(float) && float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatResult))
				result = floatResult;
			else if (type == typeof(double) && double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleResult))
				result = doubleResult;
			else if (type == typeof(char) && char.TryParse(value, out var charResult))
				result = charResult;
			else if (type == typeof(string))
				result = value;
			else if (type.IsEnum && Enum.TryParse(type, value, out var enumResult))
				result = enumResult;
			else if (type == typeof(Vector2) && YamlSerializer.TryParseVector2(value, out var vector2Result))
				result = vector2Result;
			else if (type == typeof(Vector3) && YamlSerializer.TryParseVector3(value, out var vector3Result))
				result = vector3Result;
			else if (type == typeof(Quaternion) && YamlSerializer.TryParseQuaternion(value, out var quaternionResult))
				result = quaternionResult;
			else if (type == typeof(Color) && YamlSerializer.TryParseColor(value, out var colorResult))
				result = colorResult;

			if (result == null && !silent)
				Console.WriteLine($"Unable to parse '{value}' into type '{type.Name}'");

			return result != null;
		}

		private static bool TryParseVector2(string value, out Vector2? result)
		{
			var segments = value.Split(" ", StringSplitOptions.RemoveEmptyEntries)
				.Select(segment => YamlSerializer.TryDeserializeSimple(typeof(float), segment, out var parsed) ? parsed : null)
				.OfType<float>()
				.ToArray();

			result = segments.Length switch
			{
				0 => Vector2.Zero,
				1 => new(segments[0], segments[0]),
				_ => new(segments[0], segments[1])
			};

			return true;
		}

		private static bool TryParseVector3(string value, out Vector3? result)
		{
			var segments = value.Split(" ", StringSplitOptions.RemoveEmptyEntries)
				.Select(segment => YamlSerializer.TryDeserializeSimple(typeof(float), segment, out var parsed) ? parsed : null)
				.OfType<float>()
				.ToArray();

			result = segments.Length switch
			{
				0 => Vector3.Zero,
				1 => new Vector3(segments[0], segments[0], segments[0]),
				2 => null,
				_ => new Vector3(segments[0], segments[1], segments[2])
			};

			return result != null;
		}

		private static bool TryParseColor(string value, out Color? result)
		{
			var segments = value.Split(" ", StringSplitOptions.RemoveEmptyEntries)
				.Select(segment => YamlSerializer.TryDeserializeSimple(typeof(float), segment, out var parsed) ? parsed : null)
				.OfType<float>()
				.ToArray();

			result = segments.Length switch
			{
				0 => Color.Transparent,
				1 => new(segments[0], segments[0], segments[0]),
				2 => new(segments[0], segments[0], segments[0], segments[1]),
				3 => new(segments[0], segments[1], segments[2]),
				_ => new(segments[0], segments[1], segments[2], segments[3])
			};

			return true;
		}

		private static bool TryParseQuaternion(string value, out Quaternion? result)
		{
			var segments = value.Split(" ", StringSplitOptions.RemoveEmptyEntries)
				.Select(segment => YamlSerializer.TryDeserializeSimple(typeof(float), segment, out var parsed) ? parsed : null)
				.OfType<float>()
				.ToArray();

			if (segments.Length == 0)
				result = Quaternion.Identity;
			else
			{
				result = Quaternion.RotationYawPitchRoll(
					MathUtil.DegreesToRadians(segments[0]),
					segments.Length > 1 ? 0 : MathUtil.DegreesToRadians(segments[1]),
					segments.Length > 2 ? 0 : MathUtil.DegreesToRadians(segments[2])
				);
			}

			return true;
		}

		private static string[] SplitValue(string value)
		{
			var chunks = new List<string>();
			var chunkStart = 0;

			for (var i = 0; i < value.Length; i++)
			{
				if (value[i] == ',')
					chunks.Add(value.Substring(chunkStart, i - chunkStart).Trim());
				else if (i + 1 == value.Length)
					chunks.Add(value[chunkStart..].Trim());
				else if (value[i] == '"' || value[i] == '\'')
					i = value.IndexOf(value[i], i + 1);
			}

			return chunks.ToArray();
		}
	}
}
