namespace StrideTest
{
	using Assets;
	using Ecs;
	using Ecs.Components;
	using Stride.Core.Mathematics;
	using Stride.Engine;
	using Stride.Games;
	using Stride.Input;
	using Stride.Rendering.UI;
	using Stride.UI;
	using System;
	using System.Linq;

	public class GameMain : Game
	{
		private const double TickRate = 1000.0 / 50;

		private double tickAccumulator;

		private World? world;
		private UIRenderFeature? uiRenderFeature;
		private UIElement? uiElementUnderMouseCursor;
		private Transform? cameraTransform;

		protected override void BeginRun()
		{
			this.Window.AllowUserResizing = true;

			this.SceneSystem.GraphicsCompositor = GraphicsCompositorBuilder.Create();
			this.SceneSystem.SceneInstance = new(this.Services, new());

			var assetManager = new AssetManager(this, "Base");

			this.world = new(assetManager, new(assetManager.EntityLibrary.Values), this.SceneSystem.SceneInstance.RootScene);

			var cameraEntity = this.world.Spawn("CameraDefault");
			cameraEntity.GetComponent<Camera>()?.Activate(this.SceneSystem);
			this.cameraTransform = cameraEntity.GetComponent<Transform>();

			if (this.cameraTransform != null)
			{
				this.cameraTransform.Position = new(6, 6, 6);

				this.cameraTransform.Rotation = Quaternion.RotationYawPitchRoll(
					MathUtil.DegreesToRadians(45),
					MathUtil.DegreesToRadians(-30),
					MathUtil.DegreesToRadians(0)
				);
			}

			this.world.Spawn("TestGround");
			this.world.Spawn("EnvironmentDefault");
			this.world.Spawn("TestPlayground");
			this.world.Spawn("TestModel");

			var particlesEntity = this.world.Spawn("TestParticles");
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

		protected override bool BeginDraw()
		{
			// TODO this is a hack to get around the ui being unaware of the mouse leaving it 
			this.uiRenderFeature ??= this.SceneSystem.GraphicsCompositor.RenderFeatures.OfType<UIRenderFeature>().FirstOrDefault();
			this.uiElementUnderMouseCursor = this.uiRenderFeature?.UIElementUnderMouseCursor;

			this.HandleInput();

			return base.BeginDraw();
		}

		private void HandleInput()
		{
			if (this.cameraTransform == null)
				return;

			var input = this.Input;

			if (input.IsMousePositionLocked)
			{
				var rotation = this.cameraTransform.Rotation.YawPitchRoll;
				var limit = MathUtil.DegreesToRadians(89);
				rotation.X -= input.MouseDelta.X * 4;
				rotation.Y = Math.Clamp(rotation.Y - input.MouseDelta.Y * 4, -limit, limit);
				this.cameraTransform.Rotation = Quaternion.RotationYawPitchRoll(rotation.X, rotation.Y, rotation.Z);
			}

			var movement = new Vector3();

			if (input.IsKeyDown(Keys.W))
				movement.Z--;

			if (input.IsKeyDown(Keys.A))
				movement.X--;

			if (input.IsKeyDown(Keys.S))
				movement.Z++;

			if (input.IsKeyDown(Keys.D))
				movement.X++;

			if (input.IsKeyDown(Keys.Space))
				movement.Y++;

			if (input.IsKeyDown(Keys.LeftCtrl))
				movement.Y--;

			if (movement.Length() != 0)
				this.cameraTransform.Position += Vector3.Transform(Vector3.Normalize(movement) / 4, this.cameraTransform.Rotation);

			if (input.PressedKeys.Contains(Keys.Q))
			{
				if (input.IsMousePositionLocked)
					input.UnlockMousePosition();
				else
					input.LockMousePosition();
			}
		}

		protected override void EndDraw(bool present)
		{
			// TODO this is a hack to get around the ui being unaware of the mouse leaving it 
			if (this.uiElementUnderMouseCursor != null && this.uiRenderFeature?.UIElementUnderMouseCursor == null)
				this.uiElementUnderMouseCursor?.GetType().GetProperty("MouseOverState")?.SetValue(this.uiElementUnderMouseCursor, MouseOverState.MouseOverNone);

			base.EndDraw(present);
		}
	}
}
