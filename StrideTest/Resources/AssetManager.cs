namespace StrideTest.Resources
{
	using Stride.Engine;
	using Stride.Graphics;
	using Stride.Graphics.Font;
	using System;
	using System.Collections.Generic;
	using System.IO;

	public class AssetManager
	{
		private readonly Game game;
		public GraphicsDevice GraphicsDevice => this.game.GraphicsDevice;
		private readonly Dictionary<string, object> assets = new();
		private readonly Dictionary<string, List<object>> pathHolders = new();
		private readonly Dictionary<object, List<string>> holderPaths = new();

		public AssetManager(Game game)
		{
			this.game = game;
		}

		public T? Load<T>(string path, object holder)
		{
			if (!this.assets.ContainsKey(path))
			{
				var asset = this.Load<T>(path);

				if (asset == null)
					return default;

				this.assets.Add(path, asset);
				this.pathHolders.Add(path, new());
			}

			if (!this.holderPaths.ContainsKey(holder))
				this.holderPaths.Add(holder, new());

			if (!this.pathHolders[path].Contains(holder))
				this.pathHolders[path].Add(holder);

			if (this.holderPaths[holder].Contains(path))
				this.holderPaths[holder].Add(path);

			return (T)this.assets[path];
		}

		public void Dispose(object holder)
		{
			if (!this.holderPaths.ContainsKey(holder))
				return;

			foreach (var path in this.holderPaths[holder])
			{
				this.pathHolders[path].Remove(holder);

				if (this.pathHolders[path].Count > 0)
					continue;

				this.pathHolders.Remove(path);

				if (this.assets[path] is IDisposable disposable)
					disposable.Dispose();

				this.assets.Remove(path);
			}

			this.holderPaths.Remove(holder);
		}

		private object? Load<T>(string path)
		{
			if (typeof(T) == typeof(Texture))
				return Texture.Load(this.game.GraphicsDevice, File.OpenRead($"Assets/{path}.png"));

			if (typeof(T) == typeof(Font))
				return Font.Load((FontSystem)this.game.Font, path, File.OpenRead($"Assets/{path}.ttf"));

			throw new NotSupportedException();
		}
	}
}
