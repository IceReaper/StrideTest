namespace StrideTest.Assets.Yaml
{
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;

	public static class YamlParser
	{
		public static Dictionary<string, YamlNode> Read(string text)
		{
			return YamlParser.Read(text.Replace("\r", "").Split("\n").ToList(), 0, new());
		}

		private static YamlNode Read(IList<string> lines, int currentIndentation, YamlNode currentNode)
		{
			while (lines.Any())
			{
				var line = lines.First();

				if (line.Trim().Length == 0 || line.TrimStart()[0] == '#')
				{
					lines.RemoveAt(0);

					continue;
				}

				var lineIndentation = 0;

				foreach (var character in line)
				{
					if (character == '\t')
						throw new SyntaxErrorException("Invalid indentation detected");

					if (character != ' ')
						break;

					lineIndentation++;
				}

				if (lineIndentation > currentIndentation)
					throw new SyntaxErrorException("Invalid indentation detected");

				if (lineIndentation < currentIndentation)
					return currentNode;

				line = line[lineIndentation..];
				lines.RemoveAt(0);

				string key;

				switch (line[0])
				{
					case ':':
						throw new SyntaxErrorException("Key cannot be empty");

					case '"':
					case '\'':
					{
						var end = line.IndexOf(line[0], 1);

						if (end == -1)
							throw new SyntaxErrorException("Missing end-quote for key");

						key = line.Substring(1, end - 2).Trim();

						if (key == "")
							throw new SyntaxErrorException("Key cannot be empty");

						line = line[end..].TrimStart();

						break;
					}

					default:
					{
						var end = line.IndexOfAny(new[] { '#', ':' });
						end = end == -1 ? line.Length : end;
						key = line[..end].TrimEnd();
						line = line[end..];

						break;
					}
				}

				if (line.Length == 0 || line[0] != ':')
					throw new SyntaxErrorException("Missing colon");

				line = line[1..].TrimStart();

				string? value = null;

				for (var i = 0; i < line.Length;)
				{
					if (line[i] == '"' || line[i] == '\'')
					{
						var end = line.IndexOf(line[i], 1);

						if (end == -1)
							throw new SyntaxErrorException("Missing end-quote for value");

						i = end;
					}
					else if (line[i] == ':')
						throw new SyntaxErrorException("Value cannot contain colon");
					else if (line[i] == '#')
					{
						value = line[..i].Trim();

						break;
					}
					else
						i++;

					if (i == line.Length)
						value = line.Trim();
				}

				currentNode.Add(key, YamlParser.Read(lines, currentIndentation + 2, new() { Value = value }));
			}

			return currentNode;
		}
	}
}
