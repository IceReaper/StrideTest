namespace StrideTest.Ecs.Components
{
	using Events;
	using Stride.Core.Mathematics;
	using System.IO;

	public class RotateAroundSceneInfo : SingleComponentInfo<RotateAroundScene>
	{
		public readonly Color Color = Color.White;
		public readonly float Intensity = 1;
	}

	public class RotateAroundScene : SingleComponent<RotateAroundSceneInfo>, IOnSpawn, IOnUpdate
	{
		private Transform? transform;
		private long tick;

		public RotateAroundScene(Entity entity, RotateAroundSceneInfo info)
			: base(entity, info)
		{
		}

		void IOnSpawn.OnSpawn()
		{
			this.transform = this.Entity.GetComponent<Transform>();
		}

		void IOnUpdate.OnUpdate()
		{
			if (this.transform == null)
				return;

			this.tick++;
			this.transform.Position = (Matrix.Translation(0, 1, 1) * Matrix.RotationY(MathUtil.DegreesToRadians(this.tick))).TranslationVector;
		}

		public override void Load(BinaryReader reader)
		{
			this.tick = reader.ReadInt64();
		}

		public override void Save(BinaryWriter writer)
		{
			writer.Write(this.tick);
		}
	}
}
