namespace StrideTest.Ecs.Components
{
	using Events;
	using Stride.Core.Mathematics;
	using Stride.Engine;
	using Stride.Rendering.Lights;
	using Entity = Ecs.Entity;

	public abstract class LightInfo : MultipleComponentInfo<Light>
	{
		public readonly Color Color = Color.White;
		public readonly float Intensity = 1;
	}

	public abstract class Light : MultipleComponent<LightInfo>, IOnSpawn
	{
		protected readonly Entity StrideLightEntity;

		protected Light(Entity entity, LightInfo info)
			: base(entity, info)
		{
			entity.StrideEntity.AddChild(this.StrideLightEntity = new());
		}

		void IOnSpawn.OnSpawn()
		{
			this.StrideLightEntity.Add(new LightComponent { Type = this.CreateLight(), Intensity = this.Info.Intensity });
		}

		protected abstract ILight CreateLight();
	}
}
