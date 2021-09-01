namespace StrideTest.Ecs.Components
{
	using Stride.Engine;
	using Stride.Engine.Processors;
	using System.IO;
	using Entity = Ecs.Entity;

	public class CameraInfo : SingleComponentInfo<Camera>
	{
		public readonly CameraProjectionMode ProjectionMode = CameraProjectionMode.Perspective;
	}

	public class Camera : SingleComponent<CameraInfo>
	{
		private readonly CameraComponent strideCameraComponent;

		public Camera(Entity entity, CameraInfo info)
			: base(entity, info)
		{
			entity.StrideEntity.Components.Add(this.strideCameraComponent = new() { Projection = info.ProjectionMode });
		}

		public void Activate(SceneSystem sceneSystem)
		{
			// TODO avoid using the sceneSystem here!
			this.strideCameraComponent.Slot = sceneSystem.GraphicsCompositor.Cameras[0].ToSlotId();
		}

		public override void Load(BinaryReader reader)
		{
			// TODO read a byte to determine if camera is active.
		}

		public override void Save(BinaryWriter writer)
		{
			// TODO write a byte to determine if camera is active.
		}
	}
}
