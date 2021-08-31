namespace StrideTest.Resources
{
	using System.IO;

	public abstract class LibraryBuilder
	{
		protected readonly YamlNode Yaml = new();

		public void Add(string path)
		{
			foreach (var file in Directory.GetFiles(path, "*.yaml", SearchOption.AllDirectories))
			{
				// TODO add an option here to merge nodes.
				// TODo add an option here to remove nodes.
				foreach (var (actor, components) in YamlParser.Read(File.ReadAllText(file)))
					this.Yaml.Nodes.Add(actor, components);
			}
		}
	}
}
