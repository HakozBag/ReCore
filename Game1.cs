using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Caelum_ReCore
{
    public class Platform
    {
        public Rectangle Bounds;
        public Texture2D Texture;

        public Platform(int x, int y, int width, int height, Texture2D texture)
        {
            Bounds = new Rectangle(x, y, width, height);
            Texture = texture;
        }
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _backgroundTexture;
        private Texture2D _textureIdle1, _textureIdle2, _textureJump, _textureRun1, _textureRun2, _activeTexture;
        private Texture2D _platformTexture;
        private List<Platform> _platforms = new List<Platform>();

        // HP System
        private int _currentHp = 100;
        private float _recoveryTimer = 0f;
        private Dictionary<int, Texture2D> _hpTextures = new Dictionary<int, Texture2D>();

        private Vector2 _playerPosition;
        private float _playerSpeed = 280f;
        private float _jumpForce = -700f;

        private int _finalWidth = 110;
        private int _finalHeight = 150;
        private int _feetYOffset = 100;

        private float _velocityY = 0f;
        private float _gravity = 1600f;
        private bool _isGrounded = true;
        private float _fallStartY = 0f;

        private float _animationTimer = 0f;
        private float _animationDelay = 0.15f;
        private bool _useAltFrame = false;
        private SpriteEffects _spriteFlip = SpriteEffects.None;

        private KeyboardState _previousKeyboardState;
        private float _jungleFloorSurfaceY = 670f;
        private Platform _currentPlatform = null;
        private Platform _ignoredPlatform = null;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
            _playerPosition = new Vector2(640f, _jungleFloorSurfaceY - _feetYOffset);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _backgroundTexture = Content.Load<Texture2D>("JUNGLE");
            _textureIdle1 = Content.Load<Texture2D>("Ide1");
            _textureIdle2 = Content.Load<Texture2D>("Idle2");
            _textureJump = Content.Load<Texture2D>("JUMP");
            _textureRun1 = Content.Load<Texture2D>("Run1");
            _textureRun2 = Content.Load<Texture2D>("Run2");
            _platformTexture = Content.Load<Texture2D>("RIGHTplatform1");

            for (int i = 10; i <= 100; i += 10)
            {
                _hpTextures[i] = Content.Load<Texture2D>("hp" + i);
            }

            _activeTexture = _textureIdle1;
            _platforms.Add(new Platform(100, 520, 200, 30, _platformTexture));
            _platforms.Add(new Platform(400, 450, 200, 30, _platformTexture));
            _platforms.Add(new Platform(700, 380, 200, 30, _platformTexture));
            _platforms.Add(new Platform(1000, 310, 200, 30, _platformTexture));
        }

        private void UpdatePenalties()
        {
            int lostSegments = (100 - _currentHp) / 10;
            _playerSpeed = Math.Max(100f, 280f - (lostSegments * 5f));
            _jumpForce = Math.Min(-300f, -700f + (lostSegments * 10f));
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();

            KeyboardState kState = Keyboard.GetState();
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _recoveryTimer += deltaTime;
            if (_recoveryTimer >= 20f && _currentHp < 100)
            {
                _currentHp = Math.Min(100, _currentHp + 10);
                _recoveryTimer = 0f;
                UpdatePenalties();
            }

            bool isMoving = false;
            if (kState.IsKeyDown(Keys.A)) { _playerPosition.X -= _playerSpeed * deltaTime; isMoving = true; _spriteFlip = SpriteEffects.FlipHorizontally; }
            if (kState.IsKeyDown(Keys.D)) { _playerPosition.X += _playerSpeed * deltaTime; isMoving = true; _spriteFlip = SpriteEffects.None; }

            if (kState.IsKeyDown(Keys.Space) && _previousKeyboardState.IsKeyUp(Keys.Space) && _isGrounded)
            {
                _velocityY = _jumpForce;
                _isGrounded = false;
                _currentPlatform = null;
                _fallStartY = _playerPosition.Y;
            }

            if (kState.IsKeyDown(Keys.S) && _previousKeyboardState.IsKeyUp(Keys.S) && _currentPlatform != null)
            {
                _ignoredPlatform = _currentPlatform;
                _currentPlatform = null;
                _isGrounded = false;
                _velocityY = 200f;
                _fallStartY = _playerPosition.Y;
            }

            if (!_isGrounded) _velocityY += _gravity * deltaTime;
            _playerPosition.Y += _velocityY * deltaTime;

            Rectangle playerFeet = new Rectangle((int)_playerPosition.X + 35, (int)_playerPosition.Y + _feetYOffset, 40, 15);

            if (_velocityY >= 0f && _currentPlatform == null)
            {
                foreach (var platform in _platforms)
                {
                    if (platform == _ignoredPlatform) continue;
                    if (playerFeet.Intersects(platform.Bounds) && (playerFeet.Bottom - _velocityY * deltaTime) <= platform.Bounds.Top + 20)
                    {
                        float fallDistance = _playerPosition.Y - _fallStartY;
                        if (fallDistance > 300)
                        {
                            _currentHp = Math.Max(0, _currentHp - (fallDistance > 500 ? 20 : 10));
                            UpdatePenalties();
                        }
                        _velocityY = 0f; _isGrounded = true; _currentPlatform = platform; _playerPosition.Y = platform.Bounds.Top - _feetYOffset;
                        break;
                    }
                }
            }

            if (_playerPosition.Y + _feetYOffset >= _jungleFloorSurfaceY)
            {
                if (!_isGrounded)
                {
                    float fallDistance = (_jungleFloorSurfaceY - _feetYOffset) - _fallStartY;
                    if (fallDistance > 300)
                    {
                        _currentHp = Math.Max(0, _currentHp - (fallDistance > 500 ? 20 : 10));
                        UpdatePenalties();
                    }
                }
                _playerPosition.Y = _jungleFloorSurfaceY - _feetYOffset;
                _velocityY = 0f; _isGrounded = true; _currentPlatform = null;
            }

            _playerPosition.X = Math.Clamp(_playerPosition.X, 0f, _graphics.PreferredBackBufferWidth - _finalWidth);
            _animationTimer += deltaTime;
            if (_animationTimer >= _animationDelay) { _animationTimer = 0f; _useAltFrame = !_useAltFrame; }
            _activeTexture = !_isGrounded ? _textureJump : (isMoving ? (_useAltFrame ? _textureRun2 : _textureRun1) : (_useAltFrame ? _textureIdle2 : _textureIdle1));

            _previousKeyboardState = kState;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            _spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, 1280, 720), Color.White);
            foreach (var platform in _platforms) _spriteBatch.Draw(platform.Texture, platform.Bounds, Color.White);
            _spriteBatch.Draw(_activeTexture, new Rectangle((int)_playerPosition.X, (int)_playerPosition.Y, _finalWidth, _finalHeight), null, Color.White, 0f, Vector2.Zero, _spriteFlip, 0f);

            int displayHp = (_currentHp / 10) * 10;
            if (displayHp >= 10 && _hpTextures.ContainsKey(displayHp))
                _spriteBatch.Draw(_hpTextures[displayHp], new Vector2(2, 2), null, Color.White, 0f, Vector2.Zero, 0.1f, SpriteEffects.None, 0f);

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
