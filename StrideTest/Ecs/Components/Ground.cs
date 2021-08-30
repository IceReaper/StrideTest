namespace StrideTest.Ecs.Components
{
	using Stride.Core;
	using Stride.Engine;
	using Stride.Graphics;
	using Stride.Graphics.GeometricPrimitives;
	using Stride.Rendering;
	using Stride.Rendering.Materials;
	using Stride.Rendering.Materials.ComputeColors;
	using Stride.Rendering.ProceduralModels;

	public class GroundInfo : SingleComponentInfo<Ground>
	{
	}

	public class Ground : SingleComponent<GroundInfo>
	{
		public Ground(Actor actor, GroundInfo info)
			: base(actor, info)
		{
			var groundModel = new Model();

			// TODO this is really really ugly.
			var services = new ServiceRegistry();
			services.AddService<IGraphicsDeviceService>(new GraphicsDeviceServiceLocal(actor.World.AssetManager.GraphicsDevice));

			new PlaneProceduralModel
			{
				Size = new(10, 10),
				Normal = NormalDirection.UpY,
				MaterialInstance =
				{
					Material = Material.New(
						actor.World.AssetManager.GraphicsDevice,
						new()
						{
							Attributes = new()
							{
								MicroSurface = new MaterialGlossinessMapFeature { GlossinessMap = new ComputeFloat(0.65f) },
								Diffuse = new MaterialDiffuseMapFeature(new ComputeColor(new(0.5f, 0.5f, 0.5f))),
								DiffuseModel = new MaterialDiffuseLambertModelFeature(),
								Specular = new MaterialMetalnessMapFeature { MetalnessMap = new ComputeFloat(1.0f) },
								SpecularModel = new MaterialSpecularMicrofacetModelFeature
								{
									Fresnel = new MaterialSpecularMicrofacetFresnelSchlick(),
									Visibility = new MaterialSpecularMicrofacetVisibilitySmithSchlickGGX(),
									NormalDistribution = new MaterialSpecularMicrofacetNormalDistributionGGX(),
									Environment = new MaterialSpecularMicrofacetEnvironmentGGXLUT()
								}
							}
						}
					)
				}
			}.Generate(services, groundModel);

			actor.Entity.Add(new ModelComponent(groundModel));
		}
	}
}
