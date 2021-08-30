namespace StrideTest.Ecs.Components
{
	using Stride.Engine;
	using Stride.Engine.Processors;

	public class CameraInfo : SingleComponentInfo<Camera>
	{
		public readonly CameraProjectionMode ProjectionMode = CameraProjectionMode.Orthographic;
	}

	public class Camera : SingleComponent<CameraInfo>
	{
		private readonly CameraComponent strideCameraComponent;

		public Camera(Actor actor, CameraInfo info)
			: base(actor, info)
		{
			actor.Entity.Components.Add(this.strideCameraComponent = new() { Projection = info.ProjectionMode });
		}

		public void Activate(SceneSystem sceneSystem)
		{
			this.strideCameraComponent.Slot = sceneSystem.GraphicsCompositor.Cameras[0].ToSlotId();
		}
	}
}
