using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Caelum_ReCore
{
    public class GameScreen
    {
        private Texture2D _forestSky, _forestMoon, _forestMountain, _forestBack, _forestMid, _forestLong, _forestShort, _gameLogo, _playButton, _exitButton;
        private Rectangle _playRect, _exitRect;
        private float _skyX, _moonX, _mountainX, _backX, _midX, _longX, _shortX;

        // Added to track mouse state to prevent sound spam
        private MouseState _previousMouseState;

        public bool StartGame { get; private set; }
        public bool ExitGame { get; private set; }

        public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _forestSky = content.Load<Texture2D>("forest_sky");
            _forestMoon = content.Load<Texture2D>("forest_moon");
            _forestMountain = content.Load<Texture2D>("forest_mountain");
            _forestBack = content.Load<Texture2D>("forest_back");
            _forestMid = content.Load<Texture2D>("forest_mid");
            _forestLong = content.Load<Texture2D>("forest_long");
            _forestShort = content.Load<Texture2D>("forest_short");
            _gameLogo = content.Load<Texture2D>("Game_Logo");
            _playButton = content.Load<Texture2D>("Play");
            _exitButton = content.Load<Texture2D>("Exit");

            float buttonScale = 0.35f;
            int buttonWidth = (int)(_playButton.Width * buttonScale);
            int buttonHeight = (int)(_playButton.Height * buttonScale);

            _playRect = new Rectangle((graphicsDevice.Viewport.Width - buttonWidth) / 2, 240, buttonWidth, buttonHeight);
            _exitRect = new Rectangle((graphicsDevice.Viewport.Width - buttonWidth) / 2, 320, buttonWidth, buttonHeight);
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _skyX -= 5f * dt; _moonX -= 8f * dt; _mountainX -= 12f * dt; _backX -= 18f * dt; _midX -= 25f * dt; _longX -= 35f * dt; _shortX -= 50f * dt;
            if (_skyX <= -1280) _skyX = 0; if (_moonX <= -1280) _moonX = 0; if (_mountainX <= -1280) _mountainX = 0;
            if (_backX <= -1280) _backX = 0; if (_midX <= -1280) _midX = 0; if (_longX <= -1280) _longX = 0; if (_shortX <= -1280) _shortX = 0;

            MouseState mouse = Mouse.GetState();

            // Only trigger if the button was JUST pressed (transition from Released to Pressed)
            if (mouse.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
            {
                if (_playRect.Contains(mouse.Position))
                {
                    SoundManager.PlayButtonSound(); // Play sound
                    StartGame = true;
                }
                if (_exitRect.Contains(mouse.Position))
                {
                    SoundManager.PlayButtonSound(); // Play sound
                    ExitGame = true;
                }
            }

            _previousMouseState = mouse;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle screen = new Rectangle(0, 0, spriteBatch.GraphicsDevice.Viewport.Width, spriteBatch.GraphicsDevice.Viewport.Height);
            DrawScrollingLayer(spriteBatch, _forestSky, _skyX, screen);
            DrawScrollingLayer(spriteBatch, _forestMoon, _moonX, screen);
            DrawScrollingLayer(spriteBatch, _forestMountain, _mountainX, screen);
            DrawScrollingLayer(spriteBatch, _forestBack, _backX, screen);
            DrawScrollingLayer(spriteBatch, _forestMid, _midX, screen);
            DrawScrollingLayer(spriteBatch, _forestLong, _longX, screen);
            DrawScrollingLayer(spriteBatch, _forestShort, _shortX, screen);

            spriteBatch.Draw(_gameLogo, new Rectangle((spriteBatch.GraphicsDevice.Viewport.Width - 600) / 2, 40, 600, 220), Color.White);
            spriteBatch.Draw(_playButton, _playRect, Color.White);
            spriteBatch.Draw(_exitButton, _exitRect, Color.White);
        }

        private void DrawScrollingLayer(SpriteBatch spriteBatch, Texture2D texture, float x, Rectangle screen)
        {
            spriteBatch.Draw(texture, new Rectangle((int)x, 0, screen.Width, screen.Height), Color.White);
            spriteBatch.Draw(texture, new Rectangle((int)x + screen.Width, 0, screen.Width, screen.Height), Color.White);
        }
    }
}
