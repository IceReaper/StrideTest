namespace StrideTest.Ecs.Components
{
	using Stride.Rendering.Colors;
	using Stride.Rendering.Lights;

	public class PointLightInfo : LightInfo
	{
		public readonly float Radius;

		protected override Light Create(Entity entity)
		{
			return new PointLight(entity, this);
		}
	}

	public class PointLight : Light
	{
		public new readonly PointLightInfo Info;

		public PointLight(Entity entity, PointLightInfo info)
			: base(entity, info)
		{
			this.Info = info;
		}

		protected override ILight CreateLight()
		{
			return new LightPoint
			{
				Color = new ColorRgbProvider(this.Info.Color),
				Radius = this.Info.Radius,
				Shadow =
				{
					// TODO seems to not work
					Enabled = true,
					Size = LightShadowMapSize.XLarge,
					Filter = new LightShadowMapFilterTypePcf { FilterSize = LightShadowMapFilterTypePcfSize.Filter7x7 }
				}
			};
		}
	}
}
