using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using FontBuddyLib;
using GameTimer;
using HadoukInput;

namespace ControllerDirectionTest
{
	/// <summary>
	/// this dude verifies that all the controller wrapper is wrapping things for the inputwrapper correctly
	/// checks all controllers are being checked correctly
	/// checks the forward/back is being checked correctly
	/// checks that the scrubbed/powercurve is working correctly
	/// </summary>
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		#region Members

		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		/// <summary>
		/// A font buddy we will use to write out to the screen
		/// </summary>
		private FontBuddy _text = new FontBuddy();

		/// <summary>
		/// THe controller object we gonna use to test
		/// </summary>
		private ControllerWrapper _controller;

		/// <summary>
		/// The timers we are gonna use to time the button down events
		/// </summary>
		private CountdownTimer[] _ButtonTimer;

		private InputState m_Input = new InputState();

		/// <summary>
		/// The controller index of this player
		/// </summary>
		private PlayerIndex _player = PlayerIndex.One;

		private bool _flipped = false;

		DeadZoneSample Left { get; set; }

		DeadZoneSample Right { get; set; }

		#endregion //Members

		#region Methods

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft;
			Content.RootDirectory = "Content";
			graphics.PreferredBackBufferWidth = 1024;
			graphics.PreferredBackBufferHeight = 768;
			graphics.IsFullScreen = false;

			_controller = new ControllerWrapper(PlayerIndex.One, true);
			_ButtonTimer = new CountdownTimer[(int)EKeystroke.RTriggerRelease + 1];

			for (int i = 0; i < ((int)EKeystroke.RTriggerRelease + 1); i++)
			{
				_ButtonTimer[i] = new CountdownTimer();
			}

			Left = new DeadZoneSample(new Vector2(512.0f, 256.0f), _controller.Thumbsticks.LeftThumbstick);
			Right = new DeadZoneSample(new Vector2(768.0f, 256.0f), _controller.Thumbsticks.RightThumbstick);
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			// TODO: use this.Content to load your game content here
#if OUYA
			_text.LoadContent(Content, "ArialBlack14");
#else
			_text.LoadContent(Content, "ArialBlack10");
#endif
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// Allows the game to exit
			if ((GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) ||
				Keyboard.GetState().IsKeyDown(Keys.Escape))
			{
				this.Exit();
			}

			//Update the controller
			m_Input.Update();
			_controller.Update(m_Input);

			//check if the player wants to face a different direction
			if (CheckKeyDown(m_Input, Keys.Q))
			{
				_flipped = !_flipped;
			}

			//check if the player wants to switch between scrubbed/powercurve
			if (CheckKeyDown(m_Input, Keys.W))
			{
				DeadZoneType thumbstick = _controller.Thumbsticks.ThumbstickScrubbing;
				thumbstick++;
				if (thumbstick > DeadZoneType.PowerCurve)
				{
					thumbstick = DeadZoneType.Axial;
				}
				_controller.Thumbsticks.ThumbstickScrubbing = thumbstick;
			}

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			spriteBatch.Begin();

			Vector2 position = new Vector2(graphics.GraphicsDevice.Viewport.TitleSafeArea.Left, graphics.GraphicsDevice.Viewport.TitleSafeArea.Top);
			
			//say what controller we are checking
			_text.Write("Controller Index: " + _player.ToString(), position, Justify.Left, 1.0f, Color.White, spriteBatch);
			position.Y += _text.Font.LineSpacing;

			//is the controller plugged in?
			_text.Write("Controller Plugged In: " + _controller.ControllerPluggedIn.ToString(), position, Justify.Left, 1.0f, Color.White, spriteBatch);
			position.Y += _text.Font.LineSpacing;

			//are we using the keyboard?
			_text.Write("Use Keyboard: " + _controller.UseKeyboard.ToString(), position, Justify.Left, 1.0f, Color.White, spriteBatch);
			position.Y += _text.Font.LineSpacing;

			//say what type of thumbstick scrubbing we are doing
			_text.Write("Thumbstick type: " + _controller.Thumbsticks.ThumbstickScrubbing.ToString(), position, Justify.Left, 1.0f, Color.White, spriteBatch);
			position.Y += _text.Font.LineSpacing;

			//what direction is the player facing
			_text.Write("Player is facing: " + (_flipped ? "left" : "right"), position, Justify.Left, 1.0f, Color.White, spriteBatch);
			position.Y += _text.Font.LineSpacing * 2;

			//draw the current state of each keystroke
			WriteKeyStroke(EKeystroke.Up, position);
			position.Y += _text.Font.LineSpacing;
			WriteKeyStroke(EKeystroke.Down, position);
			position.Y += _text.Font.LineSpacing;
			WriteKeyStroke(EKeystroke.Forward, position);
			position.Y += _text.Font.LineSpacing;
			WriteKeyStroke(EKeystroke.Back, position);
			position.Y += _text.Font.LineSpacing * 2;
			WriteKeyStroke(EKeystroke.Neutral, position);
			position.Y += _text.Font.LineSpacing;

			//write the dot product of the two thumbsticks
			_text.Write("dot product: " + Vector2.Dot(Left.ThumbStick.Direction, Right.ThumbStick.Direction).ToString(),
				position, Justify.Left, 1.0f, Color.White, spriteBatch);
			position.Y += _text.Font.LineSpacing * 2;

			//draw the thumbsticks
			Left.Draw(_text, spriteBatch, graphics.GraphicsDevice);
			Right.Draw(_text, spriteBatch, graphics.GraphicsDevice);

			spriteBatch.End();

			base.Draw(gameTime);
		}

		/// <summary>
		/// Check if a keyboard key was pressed this update
		/// </summary>
		/// <param name="rInputState">current input state</param>
		/// <param name="i">controller index</param>
		/// <param name="myKey">key to check</param>
		/// <returns>bool: key was pressed this update</returns>
		private bool CheckKeyDown(InputState rInputState, Keys myKey)
		{
			return (rInputState.CurrentKeyboardState.IsKeyDown(myKey) && rInputState.LastKeyboardState.IsKeyUp(myKey));
		}

		private void WriteKeyStroke(EKeystroke key, Vector2 position)
		{
			//Write the name of the button
			position.X = _text.Write(key.ToString() + ": ", position, Justify.Left, 1.0f, Color.White, spriteBatch);

			//is the button currently active
			//It would be better to store this normalized vector rather than computer it every time!
			if (_controller.CheckKeystroke(key, _flipped, Vector2.Normalize(Right.ThumbStick.Direction))) 
			{
				position.X = _text.Write("held ", position, Justify.Left, 1.0f, Color.White, spriteBatch);
			}
		}

		#endregion //Methods
	}
}
