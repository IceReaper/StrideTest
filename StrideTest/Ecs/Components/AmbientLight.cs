namespace StrideTest.Ecs.Components
{
	using Stride.Rendering.Colors;
	using Stride.Rendering.Lights;

	public class AmbientLightInfo : LightInfo
	{
		protected override Light Create(Entity entity)
		{
			return new AmbientLight(entity, this);
		}
	}

	public class AmbientLight : Light
	{
		public AmbientLight(Entity entity, AmbientLightInfo info)
			: base(entity, info)
		{
		}

		protected override ILight CreateLight()
		{
			// TODO seems to not work
			return new LightAmbient { Color = new ColorRgbProvider(this.Info.Color) };
		}
	}
}
