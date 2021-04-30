using FontBuddyLib;
using GameTimer;
using HadoukInput;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PrimitiveBuddy;

namespace ControllerDirectionTest
{
	/// <summary>
	/// This is a class for drawing the deadzone of a controller.
	/// </summary>
	public class DeadZoneSample
	{
		#region Members

		/// <summary>
		/// where this deadzone example is going to be draw on the screen.
		/// This is the very center of the thing
		/// </summary>
		/// <value>The location.</value>
		public Vector2 Position { get; set; }

		/// <summary>
		/// Gets or sets the thumb stick.
		/// </summary>
		/// <value>The thumb stick.</value>
		public ThumbstickWrapper ThumbStick { get; set; }

		#endregion //Members

		/// <summary>
		/// Initializes a new instance of the <see cref="DeadZoneTest.Windows.DeadZoneSample"/> class.
		/// </summary>
		/// <param name="eDeadZone">E dead zone.</param>
		/// <param name="position">Position.</param>
		public DeadZoneSample(Vector2 position, ThumbstickWrapper ts)
		{
			this.Position = position;
			ThumbStick = ts;
		}

		/// <summary>
		/// Draw the dead zone sample.  
		/// This will draw a circle for the thumbstick, the current thumbstick location as a white circle, the deadzone
		/// The type of dead zone scrubbing will be written above the diagram
		/// </summary>
		/// <param name="text">Text.</param>
		/// <param name="mySpriteBatch">My sprite batch.</param>
		public void Draw(FontBuddy text, SpriteBatch mySpriteBatch, GraphicsDevice graphics, GameClock time)
		{
			//draw the outline of the thumbstick
			var thumbstick = new Primitive(graphics, mySpriteBatch);
			thumbstick.Circle(Position, 100, Color.White);

			//draw the thumbstick
			Vector2 thumbstickPos = ThumbStick.Direction * 100.0f;
			thumbstickPos.Y = -1 * thumbstickPos.Y; //thumbsticks are the opposite direction of drawing
			thumbstick.Circle(Position + thumbstickPos, 10, Color.Blue);

			//write the deadzone type above the thumbstick
			Vector2 textPos = new Vector2(Position.X, Position.Y - 150.0f);
			text.Write(ThumbStick.ThumbstickScrubbing.ToString(),
					   textPos, Justify.Center, 1.0f, Color.White, mySpriteBatch, time);

			//write the raw output below the thing
			textPos = new Vector2(Position.X, Position.Y + 100.0f);
			text.Write("X: " + ThumbStick.Direction.X.ToString(),
					   textPos, Justify.Center, 1.0f, Color.White, mySpriteBatch, time);

			textPos.Y += text.MeasureString("X").Y;

			text.Write("Y: " + ThumbStick.Direction.Y.ToString(),
					   textPos, Justify.Center, 1.0f, Color.White, mySpriteBatch, time);
		}
	}
}

