namespace StrideTest.Ui
{
	using Stride.Core.Mathematics;
	using Stride.Graphics;
	using Stride.UI;
	using Stride.UI.Controls;

	public class TestButton : Button
	{
		public TestButton(SpriteFont? font)
		{
			this.Width = 200;
			this.Height = 50;
			this.BackgroundColor = Color.DarkRed;

			this.Content = new TextBlock
			{
				Text = "BUTTON",
				TextColor = Color.White,
				TextSize = 20,
				Font = font,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};

			this.MouseOverStateChanged += this.MouseOverStateChangedHandler;
		}

		private void MouseOverStateChangedHandler(object sender, PropertyChangedArgs<MouseOverState> args)
		{
			this.BackgroundColor = args.NewValue == MouseOverState.MouseOverNone ? Color.DarkRed : Color.DarkGreen;
		}
	}
}
