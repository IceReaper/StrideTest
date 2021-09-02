namespace StrideTest.Assets
{
	using Ecs;
	using Fonts;
	using Materials;
	using Stride.Engine;
	using Stride.Graphics;
	using Stride.Graphics.Font;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using Yaml;

	public class AssetManager
	{
		private readonly Game game;
		private readonly string assetRoot;
		public readonly Dictionary<string, MaterialInfo> MaterialLibrary;
		public readonly Dictionary<string, EntityInfo> EntityLibrary;

		public GraphicsDevice GraphicsDevice => this.game.GraphicsDevice;

		private readonly Dictionary<string, object> assets = new();
		private readonly Dictionary<string, List<object>> pathHolders = new();
		private readonly Dictionary<object, List<string>> holderPaths = new();

		public AssetManager(Game game, string assetRoot)
		{
			this.game = game;
			this.assetRoot = assetRoot;

			this.MaterialLibrary = Materials.MaterialLibrary.Build(YamlMerger.Merge(Path.Combine(this.assetRoot, "Materials")));
			this.EntityLibrary = Entities.EntityLibrary.Build(YamlMerger.Merge(Path.Combine(this.assetRoot, "Entities")));
		}

		public T? Load<T>(string path, object holder)
		{
			if (!this.assets.ContainsKey(path))
			{
				var asset = this.Load<T>(path);

				if (asset == null)
				{
					Console.WriteLine($"Unable to load asset '{path}' for type '{typeof(T).Name}'");

					return default;
				}

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

		private object? Load<T>(string path)
		{
			if (typeof(T) == typeof(Texture))
				return Texture.Load(this.game.GraphicsDevice, File.OpenRead(Path.Combine(this.assetRoot, "Textures", $"{path}.png")));

			if (typeof(T) == typeof(Font))
				return Font.Load((FontSystem)this.game.Font, path, File.OpenRead(Path.Combine(this.assetRoot, "Fonts/", $"{path}.ttf")));

			if (typeof(T) == typeof(Material) && this.MaterialLibrary.TryGetValue(path, out var materialInfo))
				return Material.Load(this, path, materialInfo);

			return null;
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
	}
}
