namespace StrideTest.Assets.Yaml
{
	using System.Collections.Generic;

	public class YamlNode : Dictionary<string, YamlNode>
	{
		public string? Value;
	}
}
