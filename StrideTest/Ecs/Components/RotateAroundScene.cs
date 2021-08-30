namespace StrideTest.Ecs.Components
{
	using Events;
	using Stride.Core.Mathematics;

	public class RotateAroundSceneInfo : SingleComponentInfo<RotateAroundScene>
	{
		public readonly Color Color = Color.White;
		public readonly float Intensity = 1;
	}

	public class RotateAroundScene : SingleComponent<RotateAroundSceneInfo>, IOnSpawn, IOnUpdate
	{
		private Transform? transform;
		private long tick;

		public RotateAroundScene(Actor actor, RotateAroundSceneInfo info)
			: base(actor, info)
		{
		}

		void IOnSpawn.OnSpawn()
		{
			this.transform = this.Actor.GetComponent<Transform>();
		}

		void IOnUpdate.OnUpdate()
		{
			if (this.transform == null)
				return;

			this.tick++;
			this.transform.Position = (Matrix.Translation(0, 3, 3) * Matrix.RotationY(MathUtil.DegreesToRadians(this.tick))).TranslationVector;
		}
	}
}
