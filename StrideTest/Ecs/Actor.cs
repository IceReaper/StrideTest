namespace StrideTest.Ecs
{
	using Events;
	using Stride.Engine;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	public sealed class Actor
	{
		public readonly Entity Entity;
		private readonly IEnumerable<IComponent> components;

		public uint Id { get; }
		public ActorInfo Info { get; }
		public World World { get; }

		public Actor(uint id, ActorInfo info, World world)
		{
			this.Id = id;
			this.Info = info;
			this.World = world;
			this.Entity = new();

			this.components = info.ComponentInfos.Select(componentInfo => componentInfo.Create(this)).ToArray();
		}

		public Actor(BinaryReader reader, ActorInfo info, World world)
			: this(reader.ReadUInt32(), info, world)
		{
			foreach (var component in this.components)
				component.Load(reader);
		}

		public TComponent? GetComponent<TComponent>()
			where TComponent : ISingleComponent
		{
			foreach (var component in this.components)
			{
				if (component is TComponent explicitComponent)
					return explicitComponent;
			}

			return default;
		}

		public IEnumerable<TComponent> GetComponents<TComponent>()
			where TComponent : IMultipleComponent
		{
			foreach (var component in this.components)
			{
				if (component is TComponent explicitComponent)
					yield return explicitComponent;
			}
		}

		public void Update()
		{
			foreach (var component in this.GetComponents<IOnUpdate>())
				component.OnUpdate();
		}

		public void Save(BinaryWriter writer)
		{
			foreach (var component in this.components)
				component.Save(writer);
		}
	}
}
