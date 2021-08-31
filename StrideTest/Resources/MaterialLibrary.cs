namespace StrideTest.Resources
{
	using System.Collections.Generic;

	public class MaterialLibrary
	{
		private readonly Dictionary<string, YamlNode> materials;

		public MaterialLibrary(Dictionary<string, YamlNode> materials)
		{
			this.materials = materials;
		}

		public Dictionary<string, YamlNode> Get(string material)
		{
			return this.materials[material].Nodes;
		}
	}
}
