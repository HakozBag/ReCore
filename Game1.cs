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

        private Texture2D _backgroundTexture, _textureIdle1, _textureIdle2, _textureJump, _textureRun1, _textureRun2, _activeTexture, _platformTexture;
        private Texture2D _inventoryIcon, _bagIcon;
        private List<Platform> _platforms = new List<Platform>();

        private int _currentHp = 100;
        private float _recoveryTimer = 0f;
        private Dictionary<int, Texture2D> _hpTextures = new Dictionary<int, Texture2D>();

        private bool _isInventoryOpen = false;
        private Rectangle _bagBounds = new Rectangle(1220, 10, 50, 50);
        private Rectangle _inventoryBounds = new Rectangle(340, 160, 600, 400);

        private Vector2 _playerPosition;
        private float _playerSpeed = 280f, _jumpForce = -700f, _velocityY = 0f, _gravity = 1600f, _fallStartY = 0f;
        private int _finalWidth = 110, _finalHeight = 150, _feetYOffset = 100;
        private bool _isGrounded = true;
        private float _animationTimer = 0f, _animationDelay = 0.15f;
        private bool _useAltFrame = false;
        private SpriteEffects _spriteFlip = SpriteEffects.None;
        private KeyboardState _previousKeyboardState;
        private MouseState _previousMouseState;
        private float _jungleFloorSurfaceY = 670f;
        private Platform _currentPlatform = null, _ignoredPlatform = null;

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

            // Inventory Assets
            _bagIcon = Content.Load<Texture2D>("InventoryIcon");
            _inventoryIcon = Content.Load<Texture2D>("InventorySlotsIcon");

            for (int i = 10; i <= 100; i += 10) _hpTextures[i] = Content.Load<Texture2D>("hp" + i);
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
            KeyboardState kState = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (kState.IsKeyDown(Keys.Escape)) Exit();

            // Inventory UI Logic
            if (mouse.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
            {
                if (_bagBounds.Contains(mouse.Position)) _isInventoryOpen = !_isInventoryOpen;
                else if (_isInventoryOpen && !_inventoryBounds.Contains(mouse.Position)) _isInventoryOpen = false;
            }

            // Movement and Drop-down
            bool isMoving = false;
            if (kState.IsKeyDown(Keys.A)) { _playerPosition.X -= _playerSpeed * deltaTime; isMoving = true; _spriteFlip = SpriteEffects.FlipHorizontally; }
            if (kState.IsKeyDown(Keys.D)) { _playerPosition.X += _playerSpeed * deltaTime; isMoving = true; _spriteFlip = SpriteEffects.None; }

            if (kState.IsKeyDown(Keys.Space) && _previousKeyboardState.IsKeyUp(Keys.Space) && _isGrounded)
            {
                _velocityY = _jumpForce; _isGrounded = false; _currentPlatform = null; _fallStartY = _playerPosition.Y;
            }

            if (kState.IsKeyDown(Keys.S) && _previousKeyboardState.IsKeyUp(Keys.S) && _currentPlatform != null)
            {
                _ignoredPlatform = _currentPlatform; _currentPlatform = null; _isGrounded = false; _velocityY = 100f; _fallStartY = _playerPosition.Y;
            }

            if (!_isGrounded) _velocityY += _gravity * deltaTime;
            _playerPosition.Y += _velocityY * deltaTime;

            // Collision
            Rectangle playerFeet = new Rectangle((int)_playerPosition.X + 35, (int)_playerPosition.Y + _feetYOffset, 40, 15);
            if (_velocityY >= 0f)
            {
                foreach (var platform in _platforms)
                {
                    if (platform == _ignoredPlatform) continue;
                    if (playerFeet.Intersects(platform.Bounds) && (playerFeet.Bottom - _velocityY * deltaTime) <= platform.Bounds.Top + 20)
                    {
                        _velocityY = 0f; _isGrounded = true; _currentPlatform = platform; _playerPosition.Y = platform.Bounds.Top - _feetYOffset;
                        _ignoredPlatform = null; break;
                    }
                }
            }
            if (_ignoredPlatform != null && _playerPosition.Y > _ignoredPlatform.Bounds.Top + 20) _ignoredPlatform = null;

            // Floor/Clamp
            if (_playerPosition.Y + _feetYOffset >= _jungleFloorSurfaceY) { _playerPosition.Y = _jungleFloorSurfaceY - _feetYOffset; _velocityY = 0f; _isGrounded = true; _currentPlatform = null; }
            _playerPosition.X = Math.Clamp(_playerPosition.X, 0f, 1280 - _finalWidth);

            // Animation
            _animationTimer += deltaTime;
            if (_animationTimer >= _animationDelay) { _animationTimer = 0f; _useAltFrame = !_useAltFrame; }
            _activeTexture = !_isGrounded ? _textureJump : (isMoving ? (_useAltFrame ? _textureRun2 : _textureRun1) : (_useAltFrame ? _textureIdle2 : _textureIdle1));

            _previousKeyboardState = kState;
            _previousMouseState = mouse;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            _spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, 1280, 720), Color.White);
            foreach (var platform in _platforms) _spriteBatch.Draw(platform.Texture, platform.Bounds, Color.White);
            _spriteBatch.Draw(_activeTexture, new Rectangle((int)_playerPosition.X, (int)_playerPosition.Y, _finalWidth, _finalHeight), null, Color.White, 0f, Vector2.Zero, _spriteFlip, 0f);

            // UI
            if (_hpTextures.ContainsKey((_currentHp / 10) * 10)) _spriteBatch.Draw(_hpTextures[(_currentHp / 10) * 10], new Vector2(2, 2), null, Color.White, 0f, Vector2.Zero, 0.1f, SpriteEffects.None, 0f);
            _spriteBatch.Draw(_bagIcon, _bagBounds, Color.White);
            if (_isInventoryOpen) _spriteBatch.Draw(_inventoryIcon, _inventoryBounds, Color.White);

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
