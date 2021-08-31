namespace StrideTest.Resources
{
	using Ecs;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	public class ActorLibraryBuilder : LibraryBuilder
	{
		private static readonly Dictionary<string, Type> ComponentInfoTypes;

		static ActorLibraryBuilder()
		{
			var componentInfoType = typeof(IComponentInfo);
			const string? suffix = "Info";

			ActorLibraryBuilder.ComponentInfoTypes = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes().Where(type => type.IsAssignableTo(componentInfoType)))
				.ToDictionary(type => type.Name[..^suffix.Length]);
		}

		public virtual ActorLibrary Build()
		{
			var actorInfos = new List<ActorInfo>();

			foreach (var (actor, componentsYaml) in this.Yaml.Nodes)
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

				ActorLibraryBuilder.SetValue(
					context,
					field,
					value,
					v =>
					{
						if (!Serializer.TryParse(field.FieldType, v, out var result))
							throw new NotSupportedException($"Unsupported field type {fieldName}");

						return result;
					}
				);
			}
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
