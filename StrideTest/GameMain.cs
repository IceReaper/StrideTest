namespace StrideTest
{
	using Ecs;
	using Ecs.Components;
	using Resources;
	using Stride.Core.Mathematics;
	using Stride.Engine;
	using Stride.Games;
	using Stride.Rendering.UI;
	using Stride.UI;
	using System.Linq;

	public class GameMain : Game
	{
		private const double TickRate = 1000.0 / 50;

		private double tickAccumulator;
		private World? world;
		private UIRenderFeature? uiRenderFeature;
		private UIElement? uiElementUnderMouseCursor;

		protected override void BeginRun()
		{
			this.Window.AllowUserResizing = true;

			this.SceneSystem.GraphicsCompositor = GraphicsCompositorBuilder.Create();
			this.SceneSystem.SceneInstance = new(this.Services, new());

			// TODO implement VFS for asset manager.
			var assetManager = new AssetManager(this);

			this.world = new(assetManager, assetManager.ActorLibrary, this.SceneSystem.SceneInstance.RootScene);

			var cameraEntity = this.world.Spawn("camera_default");
			cameraEntity.GetComponent<Camera>()?.Activate(this.SceneSystem);
			var cameraTransform = cameraEntity.GetComponent<Transform>();

			if (cameraTransform != null)
			{
				cameraTransform.Position = new(6, 6, 6);

				cameraTransform.Rotation = Quaternion.RotationYawPitchRoll(
					MathUtil.DegreesToRadians(45),
					MathUtil.DegreesToRadians(-30),
					MathUtil.DegreesToRadians(0)
				);
			}

			this.world.Spawn("test_ground");
			this.world.Spawn("environment_default");
			this.world.Spawn("test_playground");

			var teapotEntity = this.world.Spawn("test_teapot");
			var teapotTransform = teapotEntity.GetComponent<Transform>();

			if (teapotTransform != null)
				teapotTransform.Position = new(0, 1, 0);

			var particlesEntity = this.world.Spawn("test_particles");
			var particlesTransform = particlesEntity.GetComponent<Transform>();

			if (particlesTransform != null)
				particlesTransform.Position = new(0, 2, 0);
		}

		protected override void Update(GameTime gameTime)
		{
			this.tickAccumulator += gameTime.Elapsed.TotalMilliseconds;

			while (this.tickAccumulator > GameMain.TickRate)
			{
				this.world?.Update();
				this.tickAccumulator -= GameMain.TickRate;
			}

			base.Update(gameTime);
		}

		// TODO this is a hack to get around the ui being unaware of the mouse leaving it 
		protected override bool BeginDraw()
		{
			this.uiRenderFeature ??= this.SceneSystem.GraphicsCompositor.RenderFeatures.OfType<UIRenderFeature>().FirstOrDefault();
			this.uiElementUnderMouseCursor = this.uiRenderFeature?.UIElementUnderMouseCursor;

			return base.BeginDraw();
		}

		protected override void EndDraw(bool present)
		{
			if (this.uiElementUnderMouseCursor != null && this.uiRenderFeature?.UIElementUnderMouseCursor == null)
				this.uiElementUnderMouseCursor?.GetType().GetProperty("MouseOverState")?.SetValue(this.uiElementUnderMouseCursor, MouseOverState.MouseOverNone);

			base.EndDraw(present);
		}
	}
}
