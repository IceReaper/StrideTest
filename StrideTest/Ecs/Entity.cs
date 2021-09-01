namespace StrideTest.Ecs
{
	using Events;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	public sealed class Entity
	{
		public readonly Stride.Engine.Entity StrideEntity;
		private readonly IEnumerable<IComponent> components;

		public uint Id { get; }
		public EntityInfo Info { get; }
		public World World { get; }

		public Entity(uint id, EntityInfo info, World world)
		{
			this.Id = id;
			this.Info = info;
			this.World = world;
			this.StrideEntity = new();

			this.components = info.ComponentInfos.Select(componentInfo => componentInfo.Create(this)).ToArray();
		}

		public Entity(BinaryReader reader, EntityInfo info, World world)
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
