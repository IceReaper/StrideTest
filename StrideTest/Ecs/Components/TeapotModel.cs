namespace StrideTest.Ecs.Components
{
	using Stride.Core;
	using Stride.Engine;
	using Stride.Graphics;
	using Stride.Rendering;
	using Stride.Rendering.Materials;
	using Stride.Rendering.Materials.ComputeColors;
	using Stride.Rendering.ProceduralModels;

	public class TeapotModelInfo : SingleComponentInfo<TeapotModel>
	{
		public readonly string Texture = "";
	}

	public class TeapotModel : SingleComponent<TeapotModelInfo>
	{
		public TeapotModel(Actor actor, TeapotModelInfo info)
			: base(actor, info)
		{
			var teapotModel = new Model();

			// TODO this is really really ugly.
			var services = new ServiceRegistry();
			services.AddService<IGraphicsDeviceService>(new GraphicsDeviceServiceLocal(actor.World.AssetManager.GraphicsDevice));

			new TeapotProceduralModel
			{
				MaterialInstance =
				{
					Material = Material.New(
						actor.World.AssetManager.GraphicsDevice,
						new()
						{
							Attributes = new()
							{
								MicroSurface = new MaterialGlossinessMapFeature { GlossinessMap = new ComputeFloat(0.65f) },
								Diffuse =
									new MaterialDiffuseMapFeature(
										new ComputeTextureColor(actor.World.AssetManager.Load<Texture>(info.Texture, this))
									),
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
			}.Generate(services, teapotModel);

			actor.Entity.Add(new ModelComponent(teapotModel));
		}
	}
}
