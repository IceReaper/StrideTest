namespace StrideTest.Resources
{
	using Ecs;
	using Stride.Core.Mathematics;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Reflection;

	public class ActorLibraryBuilder
	{
		private static readonly Dictionary<string, Type> ComponentInfoTypes;

		private readonly YamlNode yaml = new();

		static ActorLibraryBuilder()
		{
			var componentInfoType = typeof(IComponentInfo);
			const string? suffix = "Info";

			ActorLibraryBuilder.ComponentInfoTypes = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes().Where(type => type.IsAssignableTo(componentInfoType)))
				.ToDictionary(type => type.Name[..^suffix.Length]);
		}

		public void Add(string path)
		{
			foreach (var file in Directory.GetFiles(path, "*.yaml", SearchOption.AllDirectories))
				this.yaml.Nodes.Add(Path.GetFileNameWithoutExtension(file), YamlParser.Read(File.ReadAllText(file)));
		}

		public ActorLibrary Build()
		{
			var actorInfos = new List<ActorInfo>();

			foreach (var (actor, componentsYaml) in this.yaml.Nodes)
			{
				var componentInfos = new List<IComponentInfo>();

				foreach (var (componentName, values) in componentsYaml.Nodes)
				{
					if (!ActorLibraryBuilder.ComponentInfoTypes.TryGetValue(componentName, out var componentInfoType))
						throw new($"Unknown component {componentName}");

					if (Activator.CreateInstance(componentInfoType) is not IComponentInfo componentInfo)
						throw new($"Unable to create ComponentInfo for {componentName}");

					ActorLibraryBuilder.SetFields(componentInfo, values);

					componentInfos.Add(componentInfo);
				}

				actorInfos.Add(new(actor, componentInfos));
			}

			return new(actorInfos);
		}

		private static void SetFields(object context, YamlNode values)
		{
			var type = context.GetType();

			foreach (var (fieldName, value) in values.Nodes)
			{
				var field = type.GetField(fieldName);

				if (field == null)
					throw new($"Unknown field {fieldName}");

				// TODO for array of primitives, split by space and parse all.
				// TODO add dictionaries!
				if (field.FieldType == typeof(bool))
					ActorLibraryBuilder.SetValue(context, field, value, v => bool.Parse(v));
				else if (field.FieldType == typeof(sbyte))
					ActorLibraryBuilder.SetValue(context, field, value, v => sbyte.Parse(v, CultureInfo.InvariantCulture));
				else if (field.FieldType == typeof(byte))
					ActorLibraryBuilder.SetValue(context, field, value, v => byte.Parse(v, CultureInfo.InvariantCulture));
				else if (field.FieldType == typeof(short))
					ActorLibraryBuilder.SetValue(context, field, value, v => short.Parse(v, CultureInfo.InvariantCulture));
				else if (field.FieldType == typeof(ushort))
					ActorLibraryBuilder.SetValue(context, field, value, v => ushort.Parse(v, CultureInfo.InvariantCulture));
				else if (field.FieldType == typeof(int))
					ActorLibraryBuilder.SetValue(context, field, value, v => int.Parse(v, CultureInfo.InvariantCulture));
				else if (field.FieldType == typeof(uint))
					ActorLibraryBuilder.SetValue(context, field, value, v => uint.Parse(v, CultureInfo.InvariantCulture));
				else if (field.FieldType == typeof(long))
					ActorLibraryBuilder.SetValue(context, field, value, v => long.Parse(v, CultureInfo.InvariantCulture));
				else if (field.FieldType == typeof(ulong))
					ActorLibraryBuilder.SetValue(context, field, value, v => ulong.Parse(v, CultureInfo.InvariantCulture));
				else if (field.FieldType == typeof(float))
					ActorLibraryBuilder.SetValue(context, field, value, v => float.Parse(v, CultureInfo.InvariantCulture));
				else if (field.FieldType == typeof(double))
					ActorLibraryBuilder.SetValue(context, field, value, v => double.Parse(v, CultureInfo.InvariantCulture));
				else if (field.FieldType == typeof(string))
					ActorLibraryBuilder.SetValue(context, field, value, v => v);
				else if (field.FieldType.IsEnum)
					ActorLibraryBuilder.SetValue(context, field, value, v => Enum.Parse(field.FieldType, v));
				else if (field.FieldType == typeof(Quaternion))
					ActorLibraryBuilder.SetValue(context, field, value, ActorLibraryBuilder.ParseQuaternion);
				else if (field.FieldType == typeof(Color))
					ActorLibraryBuilder.SetValue(context, field, value, ActorLibraryBuilder.ParseColor);
				else
					throw new NotSupportedException($"Unsupported field type {fieldName}");
			}
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

		private static void SetValue(object context, FieldInfo field, YamlNode node, Func<string, object> parser)
		{
			if (node.Value != null)
			{
				field.SetValue(context, parser(node.Value));

				return;
			}

			if (Nullable.GetUnderlyingType(field.FieldType) != null)
				return;

			throw new NotSupportedException("Parameter cannot be null");
		}
	}
}
