namespace StrideTest.Ecs
{
	using Assets;
	using Events;
	using Stride.Engine;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	public sealed class World
	{
		public readonly AssetManager AssetManager;
		public readonly EntityLibrary EntityLibrary;
		private readonly Scene strideScene;

		private readonly Dictionary<uint, Entity> entities = new();
		private uint nextEntityId;

		public IEnumerable<Entity> Entities => this.entities.Values;

		public World(AssetManager assetManager, EntityLibrary entityLibrary, Scene strideScene)
		{
			this.AssetManager = assetManager;
			this.EntityLibrary = entityLibrary;
			this.strideScene = strideScene;
		}

		public Entity Spawn(string type)
		{
			var entity = new Entity(this.nextEntityId++, this.EntityLibrary.ById(type), this);
			this.entities.Add(entity.Id, entity);
			this.strideScene.Entities.Add(entity.StrideEntity);

			foreach (var component in entity.GetComponents<IOnSpawn>())
				component.OnSpawn();

			return entity;
		}

		public void Despawn(Entity entity)
		{
			foreach (var component in entity.GetComponents<IOnDespawn>())
				component.OnDespawn();

			this.strideScene.Entities.Remove(entity.StrideEntity);
			this.entities.Remove(entity.Id);
		}

		public void Update()
		{
			foreach (var entity in this.entities.Values.ToArray())
				entity.Update();
		}

		public void Load(BinaryReader reader)
		{
			this.entities.Clear();

			this.nextEntityId = reader.ReadUInt32();
			var numEntities = reader.ReadUInt32();

			for (var i = 0; i < numEntities; i++)
			{
				var entity = new Entity(reader, this.EntityLibrary.ById(reader.ReadString()), this);
				this.entities.Add(entity.Id, entity);
			}
		}

		public void Save(BinaryWriter writer)
		{
			writer.Write(this.nextEntityId);
			writer.Write(this.entities.Count);

			foreach (var (id, entity) in this.entities)
			{
				writer.Write(id);
				entity.Save(writer);
			}
		}
	}
}
