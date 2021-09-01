namespace StrideTest.Ecs.Components
{
	using Assets.Fonts;
	using Events;
	using Stride.Core.Mathematics;
	using Stride.Engine;
	using StrideTest.Ui;
	using Entity = Ecs.Entity;

	public class UiInfo : SingleComponentInfo<Ui>
	{
		public readonly Color Color = Color.White;
		public readonly float Intensity = 1;
	}

	public class Ui : SingleComponent<UiInfo>, IOnDespawn
	{
		public Ui(Entity entity, UiInfo info)
			: base(entity, info)
		{
			entity.StrideEntity.Components.Add(
				new UIComponent
				{
					Size = new(2, .5f, 1),
					Resolution = new(200, 50, 1),
					IsFullScreen = false,
					IsBillboard = false,
					Page = new() { RootElement = new TestButton(entity.World.AssetManager.Load<Font>("Roboto", this)?.Get(20)) }
				}
			);
		}

		public void OnDespawn()
		{
			this.Entity.World.AssetManager.Dispose(this);
		}
	}
}
