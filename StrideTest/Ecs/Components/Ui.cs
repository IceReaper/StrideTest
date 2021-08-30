namespace StrideTest.Ecs.Components
{
	using Resources;
	using Stride.Core.Mathematics;
	using Stride.Engine;
	using StrideTest.Ui;

	public class UiInfo : SingleComponentInfo<Ui>
	{
		public readonly Color Color = Color.White;
		public readonly float Intensity = 1;
	}

	public class Ui : SingleComponent<UiInfo>
	{
		public Ui(Actor actor, UiInfo info)
			: base(actor, info)
		{
			actor.Entity.Components.Add(
				new UIComponent
				{
					Size = new(2, .5f, 1),
					Resolution = new(200, 50, 1),
					IsFullScreen = false,
					IsBillboard = true,
					Page = new() { RootElement = new TestButton(actor.World.AssetManager.Load<Font>("Fonts/Roboto", this)?.Get(20)) }
				}
			);
		}
	}
}
