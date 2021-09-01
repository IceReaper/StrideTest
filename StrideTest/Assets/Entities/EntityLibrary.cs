namespace StrideTest.Assets.Entities
{
	using Ecs;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Yaml;

	public static class EntityLibrary
	{
		private static readonly Dictionary<string, Type> ComponentInfoTypes;

		static EntityLibrary()
		{
			var componentInfoType = typeof(IComponentInfo);
			const string? suffix = "Info";

			EntityLibrary.ComponentInfoTypes = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes().Where(type => type.IsAssignableTo(componentInfoType)))
				.ToDictionary(type => type.Name[..^suffix.Length]);
		}

		public static Dictionary<string, EntityInfo> Build(Dictionary<string, YamlNode> entitiesYaml)
		{
			var entityInfos = new Dictionary<string, EntityInfo>();

			foreach (var (entity, componentsYaml) in entitiesYaml)
			{
				var componentInfos = new List<IComponentInfo>();

				foreach (var (componentName, values) in componentsYaml)
				{
					if (!EntityLibrary.ComponentInfoTypes.TryGetValue(componentName, out var componentInfoType)
						|| Activator.CreateInstance(componentInfoType) is not IComponentInfo componentInfo)
					{
						Console.WriteLine($"Unable to add component '{componentName}' to entity '{entity}'");

						continue;
					}

					YamlSerializer.Deserialize(componentInfo, values);
					componentInfos.Add(componentInfo);
				}

				entityInfos.Add(entity, new(entity, componentInfos));
			}

			return new(entityInfos);
		}
	}
}
