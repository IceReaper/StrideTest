namespace StrideTest.Ecs
{
	using Events;
	using Resources;
	using Stride.Engine;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	public sealed class World
	{
		public readonly AssetManager AssetManager;
		public readonly ActorLibrary ActorLibrary;
		private readonly Scene strideScene;

		private readonly Dictionary<uint, Actor> actors = new();
		private uint nextActorId;

		public IEnumerable<Actor> Actors => this.actors.Values;

		public World(AssetManager assetManager, ActorLibrary actorLibrary, Scene strideScene)
		{
			this.AssetManager = assetManager;
			this.ActorLibrary = actorLibrary;
			this.strideScene = strideScene;
		}

		public Actor Spawn(string type)
		{
			var actor = new Actor(this.nextActorId++, this.ActorLibrary.ById(type), this);
			this.actors.Add(actor.Id, actor);
			this.strideScene.Entities.Add(actor.Entity);

			foreach (var component in actor.GetComponents<IOnSpawn>())
				component.OnSpawn();

			return actor;
		}

		public void Despawn(Actor actor)
		{
			foreach (var component in actor.GetComponents<IOnDespawn>())
				component.OnDespawn();

			this.strideScene.Entities.Remove(actor.Entity);
			this.actors.Remove(actor.Id);
		}

		public void Update()
		{
			foreach (var actor in this.actors.Values.ToArray())
				actor.Update();
		}

		public void Load(BinaryReader reader)
		{
			this.actors.Clear();

			this.nextActorId = reader.ReadUInt32();
			var numActors = reader.ReadUInt32();

			for (var i = 0; i < numActors; i++)
			{
				var actor = new Actor(reader, this.ActorLibrary.ById(reader.ReadString()), this);
				this.actors.Add(actor.Id, actor);
			}
		}

		public void Save(BinaryWriter writer)
		{
			writer.Write(this.nextActorId);
			writer.Write(this.actors.Count);

			foreach (var (id, actor) in this.actors)
			{
				writer.Write(id);
				actor.Save(writer);
			}
		}
	}
}
