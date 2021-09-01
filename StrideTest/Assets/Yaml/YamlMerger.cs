namespace StrideTest.Assets.Yaml
{
	using System.Collections.Generic;
	using System.IO;

	public static class YamlMerger
	{
		public static Dictionary<string, YamlNode> Merge(string path)
		{
			var result = new Dictionary<string, YamlNode>();

			foreach (var file in Directory.GetFiles(path, "*.yaml", SearchOption.AllDirectories))
			{
				foreach (var (actor, components) in YamlParser.Read(File.ReadAllText(file)))
					result.Add(actor, components);
			}

			return result;
		}
	}
}
