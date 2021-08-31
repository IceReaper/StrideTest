namespace StrideTest.Resources
{
	using SharpFont;
	using Stride.Graphics;
	using Stride.Graphics.Font;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;

	public class Font : IDisposable
	{
		private readonly FontSystem fontSystem;
		private readonly string path;
		private readonly Dictionary<int, SpriteFont> cache = new();

		private Font(FontSystem fontSystem, string path)
		{
			this.fontSystem = fontSystem;
			this.path = path;
		}

		public static Font Load(FontSystem fontSystem, string path, Stream stream)
		{
			const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

			var fontManager = fontSystem.GetType().GetProperty("FontManager", flags)?.GetValue(fontSystem);

			if (fontManager?.GetType().GetField("cachedFontFaces", flags)?.GetValue(fontManager) is not Dictionary<string, Face> cachedFontFaces
				|| fontManager.GetType().GetField("freetypeLibrary", flags)?.GetValue(fontManager) is not Library freetypeLibrary)
				return new(fontSystem, path);

			var newFontData = new byte[stream.Length];
			stream.Read(newFontData, 0, newFontData.Length);

			cachedFontFaces[FontHelper.GetFontPath(path, FontStyle.Regular)] = freetypeLibrary.NewMemoryFace(newFontData, 0);

			return new(fontSystem, path);
		}

		public SpriteFont Get(int size)
		{
			if (!this.cache.ContainsKey(size))
				this.cache.Add(size, this.fontSystem.NewDynamic(size, this.path, FontStyle.Regular));

			return this.cache[size];
		}

		public void Dispose()
		{
			foreach (var spriteFont in this.cache.Values)
				spriteFont.Dispose();

			GC.SuppressFinalize(this);
		}
	}
}
