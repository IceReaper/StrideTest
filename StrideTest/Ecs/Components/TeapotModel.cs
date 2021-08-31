namespace StrideTest.Ecs.Components
{
	using Events;
	using Resources;
	using Stride.Core;
	using Stride.Engine;
	using Stride.Graphics;
	using Stride.Rendering;
	using Stride.Rendering.ProceduralModels;

	public class TeapotModelInfo : SingleComponentInfo<TeapotModel>
	{
		public readonly string Material = "";
	}

	public class TeapotModel : SingleComponent<TeapotModelInfo>, IOnDespawn
	{
		public TeapotModel(Actor actor, TeapotModelInfo info)
			: base(actor, info)
		{
			// TODO this is really really ugly.
			var services = new ServiceRegistry();
			services.AddService<IGraphicsDeviceService>(new GraphicsDeviceServiceLocal(actor.World.AssetManager.GraphicsDevice));

			var teapotModel = new Model();

			new TeapotProceduralModel { MaterialInstance = { Material = actor.World.AssetManager.Load<PbrMaterial>(info.Material, this) } }.Generate(
				services,
				teapotModel
			);

			actor.Entity.Add(new ModelComponent(teapotModel));
		}

		void IOnDespawn.OnDespawn()
		{
			this.Actor.World.AssetManager.Dispose(this);
		}
	}
}
