namespace StrideTest.Ecs.Components
{
	using Events;
	using Stride.Core.Mathematics;
	using Stride.Engine;
	using Stride.Rendering.Lights;

	public abstract class LightInfo : MultipleComponentInfo<Light>
	{
		public readonly Color Color = Color.White;
		public readonly float Intensity = 1;
	}

	public abstract class Light : MultipleComponent<LightInfo>, IOnSpawn
	{
		protected readonly Entity LightEntity;

		protected Light(Actor actor, LightInfo info)
			: base(actor, info)
		{
			actor.Entity.AddChild(this.LightEntity = new());
		}

		void IOnSpawn.OnSpawn()
		{
			this.LightEntity.Add(new LightComponent { Type = this.CreateLight(), Intensity = this.Info.Intensity });
		}

		protected abstract ILight CreateLight();
	}
}
