using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        // Background variables
        private Texture2D _backgroundTexture;

        // Player Textures
        private Texture2D _textureIdle1;
        private Texture2D _textureIdle2;
        private Texture2D _textureJump;
        private Texture2D _textureRun1;
        private Texture2D _textureRun2;
        private Texture2D _activeTexture;

        // Platform variables
        private Texture2D _platformTexture;
        private List<Platform> _platforms = new List<Platform>();

        // Player Position variables
        private Vector2 _playerPosition;
        private float _playerSpeed;

        // Uniform Canvas Dimensions
        private int _finalWidth = 110;
        private int _finalHeight = 150;

        // --- ARTWORK PADDING ADJUSTMENT ---
        private int _feetYOffset = 115;

        // Velocity-Based Physics Variables
        private float _velocityY = 0f;
        private float _gravity = 1600f;
        private float _jumpForce = -700f;
        private bool _isGrounded = true;

        // Animation Timer variables
        private float _animationTimer = 0f;
        private float _animationDelay = 0.15f;
        private bool _useAltFrame = false;
        private SpriteEffects _spriteFlip = SpriteEffects.None;

        // Keyboard state tracking
        private KeyboardState _previousKeyboardState;

        // Level & Platform tracking mechanics
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
            _playerSpeed = 280f;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _backgroundTexture = Content.Load<Texture2D>("JUNGLE");

            _textureIdle1 = Content.Load<Texture2D>("Ide1");
            _textureIdle2 = Content.Load<Texture2D>("Idle2");
            _textureJump = Content.Load<Texture2D>("Jump");
            _textureRun1 = Content.Load<Texture2D>("Run1");
            _textureRun2 = Content.Load<Texture2D>("Run2");
            _platformTexture = Content.Load<Texture2D>("RIGHTplatform1");

            _activeTexture = _textureIdle1;

            // Lowered platforms configurations
            _platforms.Add(new Platform(100, 520, 200, 30, _platformTexture));
            _platforms.Add(new Platform(400, 450, 200, 30, _platformTexture));
            _platforms.Add(new Platform(700, 380, 200, 30, _platformTexture));
            _platforms.Add(new Platform(1000, 310, 200, 30, _platformTexture));
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState kState = Keyboard.GetState();
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            bool isMoving = false;

            // --- 1. HORIZONTAL MOVEMENT ---
            if (kState.IsKeyDown(Keys.A))
            {
                _playerPosition.X -= _playerSpeed * deltaTime;
                isMoving = true;
                _spriteFlip = SpriteEffects.FlipHorizontally;
            }
            if (kState.IsKeyDown(Keys.D))
            {
                _playerPosition.X += _playerSpeed * deltaTime;
                isMoving = true;
                _spriteFlip = SpriteEffects.None;
            }

            _playerPosition.X = Math.Clamp(_playerPosition.X, 0f, _graphics.PreferredBackBufferWidth - _finalWidth);

            // --- 2. JUMP MECHANIC ---
            if (kState.IsKeyDown(Keys.Space) && _previousKeyboardState.IsKeyUp(Keys.Space) && _isGrounded)
            {
                _velocityY = _jumpForce;
                _isGrounded = false;
                _currentPlatform = null;
            }

            // --- 3. APPLY CONSTANT GRAVITY ---
            if (!_isGrounded)
            {
                _velocityY += _gravity * deltaTime;
            }

            _playerPosition.Y += _velocityY * deltaTime;

            // Feet bounding box for collision detection calculations
            int feetWidth = 40;
            Rectangle playerFeet = new Rectangle(
                (int)_playerPosition.X + ((_finalWidth - feetWidth) / 2),
                (int)_playerPosition.Y + _feetYOffset,
                feetWidth,
                15 // Increased height slightly for more robust overlap check
            );

            // --- 4. INTERACT MECHANIC (W) ---
            if (kState.IsKeyDown(Keys.W) && _previousKeyboardState.IsKeyUp(Keys.W))
            {
                Debug.WriteLine("System: Player Interacted!");
            }

            // --- 5. DROP DOWN MECHANIC (S) ---
            if (kState.IsKeyDown(Keys.S) && _previousKeyboardState.IsKeyUp(Keys.S))
            {
                if (_currentPlatform != null)
                {
                    _ignoredPlatform = _currentPlatform;
                    _currentPlatform = null;
                    _isGrounded = false;
                    _velocityY = 200f; // Clear platform snap boundary instantly
                }
            }

            if (_ignoredPlatform != null && !playerFeet.Intersects(_ignoredPlatform.Bounds))
            {
                _ignoredPlatform = null;
            }

            // --- 6. SOLID PLATFORM LANDING ENGINE ---
            // Checking landing when velocity is strictly downward or positive
            if (_velocityY >= 0f && _currentPlatform == null)
            {
                foreach (var platform in _platforms)
                {
                    if (platform == _ignoredPlatform) continue;

                    // Expanded tolerance buffer zone (Bounds.Top + 20) to capture rapid movement frames
                    if (playerFeet.Intersects(platform.Bounds) &&
                        (playerFeet.Bottom - _velocityY * deltaTime) <= platform.Bounds.Top + 20)
                    {
                        _velocityY = 0f;
                        _isGrounded = true;
                        _currentPlatform = platform;
                        _playerPosition.Y = platform.Bounds.Top - _feetYOffset;
                        break;
                    }
                }
            }

            // Fall off platform edge check if walking off sideways
            if (_currentPlatform != null)
            {
                playerFeet = new Rectangle((int)_playerPosition.X + ((_finalWidth - feetWidth) / 2), (int)_playerPosition.Y + _feetYOffset, feetWidth, 15);
                if (!playerFeet.Intersects(_currentPlatform.Bounds))
                {
                    _currentPlatform = null;
                    _isGrounded = false;
                }
            }

            // --- 7. JUNGLE FLOOR LANDING CONSTRAINT ---
            if (_playerPosition.Y + _feetYOffset >= _jungleFloorSurfaceY)
            {
                _playerPosition.Y = _jungleFloorSurfaceY - _feetYOffset;
                _velocityY = 0f;
                _isGrounded = true;
                _currentPlatform = null;
            }

            // --- 8. ANIMATION TIMING & STATES ---
            _animationTimer += deltaTime;
            if (_animationTimer >= _animationDelay)
            {
                _animationTimer = 0f;
                _useAltFrame = !_useAltFrame;
            }

            if (!_isGrounded)
            {
                _activeTexture = _textureJump;
            }
            else if (isMoving)
            {
                _activeTexture = _useAltFrame ? _textureRun2 : _textureRun1;
            }
            else
            {
                _activeTexture = _useAltFrame ? _textureIdle2 : _textureIdle1;
            }

            _previousKeyboardState = kState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            _spriteBatch.Draw(
                _backgroundTexture,
                new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight),
                Color.White
            );

            foreach (var platform in _platforms)
            {
                _spriteBatch.Draw(platform.Texture, platform.Bounds, Color.White);
            }

            _spriteBatch.Draw(
                _activeTexture,
                new Rectangle((int)_playerPosition.X, (int)_playerPosition.Y, _finalWidth, _finalHeight),
                null,
                Color.White,
                0f,
                Vector2.Zero,
                _spriteFlip,
                0f
            );

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
