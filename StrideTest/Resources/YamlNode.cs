namespace StrideTest.Resources
{
	using System.Collections.Generic;

	public class YamlNode
	{
		public readonly Dictionary<string, YamlNode> Nodes = new();
		public string? Value;

		public YamlNode()
		{
		}

		public YamlNode(string value)
		{
			this.Value = value;
		}

		public YamlNode(Dictionary<string, YamlNode> list)
		{
			this.Nodes = list;
		}

		public YamlNode? this[string key]
		{
			get
			{
				this.Nodes.TryGetValue(key, out var result);

				return result;
			}
			set
			{
				if (this.Nodes.ContainsKey(key))
					this.Nodes[key] = value!;
				else
					this.Nodes.Add(key, value!);
			}
		}
	}
}
