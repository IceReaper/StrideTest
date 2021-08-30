namespace StrideTest.Ecs.Components
{
	using Stride.Rendering.Colors;
	using Stride.Rendering.Lights;

	public class AmbientLightInfo : LightInfo
	{
		protected override Light Create(Actor actor)
		{
			return new AmbientLight(actor, this);
		}
	}

	public class AmbientLight : Light
	{
		public AmbientLight(Actor actor, AmbientLightInfo info)
			: base(actor, info)
		{
		}

		protected override ILight CreateLight()
		{
			// TODO seems to not work
			return new LightAmbient { Color = new ColorRgbProvider(this.Info.Color) };
		}
	}
}
