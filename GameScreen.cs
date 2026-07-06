using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Caelum_ReCore
{
    public class GameScreen
    {
        private Texture2D _forestSky, _forestMoon, _forestMountain, _forestBack, _forestMid, _forestLong, _forestShort, _gameLogo;
        private Texture2D _playButton, _exitButton, _level1Btn, _level2Btn, _level3Btn, _creditsBtn, _backBtn;
        private Rectangle _playRect, _exitRect, _lvl1Rect, _lvl2Rect, _lvl3Rect, _creditsRect, _backRect;
        private float _skyX, _moonX, _mountainX, _backX, _midX, _longX, _shortX;
        private MouseState _previousMouseState;

        public bool StartGame { get; private set; }
        public bool ExitGame { get; private set; }
        public bool Level1Selected { get; private set; }
        public bool BackToTitle { get; private set; }

        public void ResetFlags()
        {
            StartGame = false;
            BackToTitle = false;
            Level1Selected = false;
        }

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
            _level1Btn = content.Load<Texture2D>("level1");
            _level2Btn = content.Load<Texture2D>("level2");
            _level3Btn = content.Load<Texture2D>("level3");
            _creditsBtn = content.Load<Texture2D>("credits");
            _backBtn = content.Load<Texture2D>("back");

            float btnScale = 0.35f;
            int w = (int)(_playButton.Width * btnScale);
            int h = (int)(_playButton.Height * btnScale);
            int centerX = (graphicsDevice.Viewport.Width - w) / 2;

            _playRect = new Rectangle(centerX, 280, w, h);
            _creditsRect = new Rectangle(centerX, 360, w, h);
            _exitRect = new Rectangle(centerX, 440, w, h);
            _lvl1Rect = new Rectangle(centerX, 150, w, h);
            _lvl2Rect = new Rectangle(centerX, 230, w, h);
            _lvl3Rect = new Rectangle(centerX, 310, w, h);
            _backRect = new Rectangle(20, 20, 80, 80);
        }

        public void Update(GameTime gameTime)
        {
            UpdateBackgrounds(gameTime);
            MouseState mouse = Mouse.GetState();
            if (mouse.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
            {
                if (_playRect.Contains(mouse.Position)) { SoundManager.PlayButtonSound(); StartGame = true; }
                if (_exitRect.Contains(mouse.Position)) { SoundManager.PlayButtonSound(); ExitGame = true; }
            }
            _previousMouseState = mouse;
        }

        public void UpdateLevelSelect(GameTime gameTime)
        {
            UpdateBackgrounds(gameTime);
            MouseState mouse = Mouse.GetState();
            if (mouse.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
            {
                if (_lvl1Rect.Contains(mouse.Position)) { SoundManager.PlayButtonSound(); Level1Selected = true; }
                if (_backRect.Contains(mouse.Position)) { SoundManager.PlayButtonSound(); BackToTitle = true; }
            }
            _previousMouseState = mouse;
        }

        private void UpdateBackgrounds(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _skyX -= 5f * dt; _moonX -= 8f * dt; _mountainX -= 12f * dt; _backX -= 18f * dt; _midX -= 25f * dt; _longX -= 35f * dt; _shortX -= 50f * dt;
            if (_skyX <= -1280) _skyX = 0; if (_moonX <= -1280) _moonX = 0; if (_mountainX <= -1280) _mountainX = 0;
            if (_backX <= -1280) _backX = 0; if (_midX <= -1280) _midX = 0; if (_longX <= -1280) _longX = 0; if (_shortX <= -1280) _shortX = 0;
        }

        public void Draw(SpriteBatch sb)
        {
            DrawBackgrounds(sb);
            sb.Draw(_gameLogo, new Rectangle((sb.GraphicsDevice.Viewport.Width - 600) / 2, 40, 600, 220), Color.White);
            sb.Draw(_playButton, _playRect, Color.White);
            sb.Draw(_creditsBtn, _creditsRect, Color.White);
            sb.Draw(_exitButton, _exitRect, Color.White);
        }

        public void DrawLevelSelect(SpriteBatch sb)
        {
            DrawBackgrounds(sb);
            sb.Draw(_level1Btn, _lvl1Rect, Color.White);
            sb.Draw(_level2Btn, _lvl2Rect, Color.White);
            sb.Draw(_level3Btn, _lvl3Rect, Color.White);
            sb.Draw(_backBtn, _backRect, Color.White);
        }

        private void DrawBackgrounds(SpriteBatch sb)
        {
            Rectangle screen = new Rectangle(0, 0, sb.GraphicsDevice.Viewport.Width, sb.GraphicsDevice.Viewport.Height);
            DrawScrollingLayer(sb, _forestSky, _skyX, screen);
            DrawScrollingLayer(sb, _forestMoon, _moonX, screen);
            DrawScrollingLayer(sb, _forestMountain, _mountainX, screen);
            DrawScrollingLayer(sb, _forestBack, _backX, screen);
            DrawScrollingLayer(sb, _forestMid, _midX, screen);
            DrawScrollingLayer(sb, _forestLong, _longX, screen);
            DrawScrollingLayer(sb, _forestShort, _shortX, screen);
        }

        private void DrawScrollingLayer(SpriteBatch sb, Texture2D tex, float x, Rectangle s)
        {
            sb.Draw(tex, new Rectangle((int)x, 0, s.Width, s.Height), Color.White);
            sb.Draw(tex, new Rectangle((int)x + s.Width, 0, s.Width, s.Height), Color.White);
        }
    }
}
