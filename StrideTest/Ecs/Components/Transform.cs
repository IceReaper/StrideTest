namespace StrideTest.Ecs.Components
{
	using Extensions;
	using Stride.Core.Mathematics;
	using Stride.Engine;
	using System.IO;

	public class TransformInfo : SingleComponentInfo<Transform>
	{
	}

	public class Transform : SingleComponent<TransformInfo>
	{
		public Vector3 Position
		{
			get => this.strideTransform.Position;
			set => this.strideTransform.Position = value;
		}

		public Quaternion Rotation
		{
			get => this.strideTransform.Rotation;
			set => this.strideTransform.Rotation = value;
		}

		public Vector3 Scale
		{
			get => this.strideTransform.Scale;
			set => this.strideTransform.Scale = value;
		}

		private readonly TransformComponent strideTransform;

		public Transform(Actor actor, TransformInfo info)
			: base(actor, info)
		{
			this.strideTransform = actor.Entity.Transform;
		}

		public override void Load(BinaryReader reader)
		{
			this.Position = reader.ReadVector3();
			this.Rotation = reader.ReadQuaternion();
			this.Scale = reader.ReadVector3();
		}

		public override void Save(BinaryWriter writer)
		{
			writer.Write(this.Position);
			writer.Write(this.Rotation);
			writer.Write(this.Scale);
		}
	}
}
