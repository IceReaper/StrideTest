namespace StrideTest.Ecs.Components
{
	using Stride.Core.Mathematics;
	using Stride.Rendering.Colors;
	using Stride.Rendering.Lights;

	public class DirectionalLightInfo : LightInfo
	{
		public readonly Quaternion Rotation = Quaternion.Identity;

		protected override Light Create(Entity entity)
		{
			return new DirectionalLight(entity, this);
		}
	}

	public class DirectionalLight : Light
	{
		public DirectionalLight(Entity entity, DirectionalLightInfo info)
			: base(entity, info)
		{
			this.StrideLightEntity.Transform.Rotation = info.Rotation;
		}

		protected override ILight CreateLight()
		{
			return new LightDirectional
			{
				Color = new ColorRgbProvider(this.Info.Color),
				Shadow =
				{
					Enabled = true,
					Size = LightShadowMapSize.XLarge,
					Filter = new LightShadowMapFilterTypePcf { FilterSize = LightShadowMapFilterTypePcfSize.Filter7x7 }
				}
			};
		}
	}
}
