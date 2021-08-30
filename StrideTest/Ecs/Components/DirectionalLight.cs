namespace StrideTest.Ecs.Components
{
	using Stride.Core.Mathematics;
	using Stride.Rendering.Colors;
	using Stride.Rendering.Lights;

	public class DirectionalLightInfo : LightInfo
	{
		public readonly Quaternion Rotation = Quaternion.Identity;

		protected override Light Create(Actor actor)
		{
			return new DirectionalLight(actor, this);
		}
	}

	public class DirectionalLight : Light
	{
		public DirectionalLight(Actor actor, DirectionalLightInfo info)
			: base(actor, info)
		{
			this.LightEntity.Transform.Rotation = info.Rotation;
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
