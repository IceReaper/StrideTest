namespace StrideTest.Assets.Materials
{
	using System;
	using System.Collections.Generic;
	using Yaml;

	public static class MaterialLibrary
	{
		public static Dictionary<string, MaterialInfo> Build(Dictionary<string, YamlNode> materialsYaml)
		{
			var result = new Dictionary<string, MaterialInfo>();

			foreach (var (materialId, materialYaml) in materialsYaml)
			{
				if (!YamlSerializer.TryDeserialize(materialYaml, out MaterialInfo? materialInfo) || materialInfo == null)
				{
					Console.WriteLine($"Unable to load material '{materialId}'");

					continue;
				}

				result.Add(materialId, materialInfo);
			}

			return result;
		}
	}
}
