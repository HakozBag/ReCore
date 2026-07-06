using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Caelum_ReCore
{
    public enum GameTitle { Title, Playing, GameOver }

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
        private GameTitle _currentState = GameTitle.Title;
        private GameScreen _menuScreen = new GameScreen();

        private Texture2D _backgroundTexture, _textureIdle1, _textureIdle2, _textureJump, _textureRun1, _textureRun2, _activeTexture, _platformTexture;
        private Texture2D _inventoryIcon, _bagIcon, _amuletTexture;
        private List<Platform> _platforms = new List<Platform>();

        private Vector2 _amuletPosition = new Vector2(1050, 270);
        private float _amuletFloatTimer = 0f;
        private bool _hasAmulet = false;
        private bool _amuletCollected = false;

        private int _currentHp = 100;
        private float _recoveryTimer = 0f;
        private Dictionary<int, Texture2D> _hpTextures = new Dictionary<int, Texture2D>();
        private bool _isInventoryOpen = false;
        private Rectangle _bagBounds = new Rectangle(1220, 10, 50, 50);
        private Rectangle _inventoryBounds = new Rectangle(440, 160, 400, 400);
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
            _fallStartY = _playerPosition.Y;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _menuScreen.LoadContent(Content, GraphicsDevice);

            _backgroundTexture = Content.Load<Texture2D>("JUNGLE");
            _textureIdle1 = Content.Load<Texture2D>("Ide1");
            _textureIdle2 = Content.Load<Texture2D>("Idle2");
            _textureJump = Content.Load<Texture2D>("JUMP");
            _textureRun1 = Content.Load<Texture2D>("Run1");
            _textureRun2 = Content.Load<Texture2D>("Run2");
            _platformTexture = Content.Load<Texture2D>("RIGHTplatform1");
            _bagIcon = Content.Load<Texture2D>("InventoryIcon");
            _inventoryIcon = Content.Load<Texture2D>("InventorySlotsIcon");
            _amuletTexture = Content.Load<Texture2D>("RecoveryAmulet");

            for (int i = 10; i <= 100; i += 10) _hpTextures[i] = Content.Load<Texture2D>("hp" + i);
            _activeTexture = _textureIdle1;

            _platforms.Add(new Platform(100, 520, 200, 30, _platformTexture));
            _platforms.Add(new Platform(400, 450, 200, 30, _platformTexture));
            _platforms.Add(new Platform(700, 380, 200, 30, _platformTexture));
            _platforms.Add(new Platform(1000, 310, 200, 30, _platformTexture));

            SoundManager.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            if (_currentState == GameTitle.Title)
            {
                SoundManager.PlayMenuMusic();
                _menuScreen.Update(gameTime);
                if (_menuScreen.StartGame)
                {
                    _currentState = GameTitle.Playing;
                    SoundManager.PlayGameMusic();
                }
                if (_menuScreen.ExitGame) Exit();
            }
            else if (_currentState == GameTitle.Playing)
            {
                UpdateGameplay(gameTime);
            }
            base.Update(gameTime);
        }

        private void UpdateGameplay(GameTime gameTime)
        {
            KeyboardState kState = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (kState.IsKeyDown(Keys.Escape)) _currentState = GameTitle.Title;

            _amuletFloatTimer += deltaTime;
            if (!_amuletCollected)
            {
                Rectangle amuletRect = new Rectangle((int)_amuletPosition.X, (int)(_amuletPosition.Y + (float)Math.Sin(_amuletFloatTimer * 3f) * 10f), 40, 40);
                Rectangle playerRect = new Rectangle((int)_playerPosition.X, (int)_playerPosition.Y, _finalWidth, _finalHeight);

                if (playerRect.Intersects(amuletRect))
                {
                    _amuletCollected = true;
                    _hasAmulet = true;
                }
            }

            _recoveryTimer += deltaTime;
            float recoveryThreshold = _hasAmulet ? 5f : 20f;
            if (_recoveryTimer >= recoveryThreshold && _currentHp < 100)
            {
                _currentHp = Math.Min(100, _currentHp + 10);
                _recoveryTimer = 0f;
                UpdatePenalties();
            }

            if (mouse.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released)
            {
                if (_bagBounds.Contains(mouse.Position)) _isInventoryOpen = !_isInventoryOpen;
                else if (_isInventoryOpen && !_inventoryBounds.Contains(mouse.Position)) _isInventoryOpen = false;
            }

            bool isMoving = false;
            if (kState.IsKeyDown(Keys.A)) { _playerPosition.X -= _playerSpeed * deltaTime; isMoving = true; _spriteFlip = SpriteEffects.FlipHorizontally; }
            if (kState.IsKeyDown(Keys.D)) { _playerPosition.X += _playerSpeed * deltaTime; isMoving = true; _spriteFlip = SpriteEffects.None; }

            if (kState.IsKeyDown(Keys.S) && _currentPlatform != null)
            {
                _ignoredPlatform = _currentPlatform;
                _isGrounded = false;
                _currentPlatform = null;
                _velocityY = 100f;
            }

            if (kState.IsKeyDown(Keys.Space) && _previousKeyboardState.IsKeyUp(Keys.Space) && _isGrounded)
            {
                _velocityY = _jumpForce; _isGrounded = false; _currentPlatform = null; _fallStartY = _playerPosition.Y;
            }

            _velocityY += _gravity * deltaTime;
            _playerPosition.Y += _velocityY * deltaTime;

            Rectangle playerFeet = new Rectangle((int)_playerPosition.X + 35, (int)_playerPosition.Y + _feetYOffset, 40, 15);
            bool hitAnything = false;

            if (_velocityY >= 0f)
            {
                foreach (var platform in _platforms)
                {
                    if (platform == _ignoredPlatform) continue;
                    if (playerFeet.Intersects(platform.Bounds) && (playerFeet.Bottom - _velocityY * deltaTime) <= platform.Bounds.Top + 20)
                    {
                        if (!_isGrounded)
                        {
                            float fallDistance = _playerPosition.Y - _fallStartY;
                            if (fallDistance > 150f)
                            {
                                int damage = ((int)(fallDistance / 150f)) * 10;
                                _currentHp = Math.Max(10, _currentHp - damage);
                                _recoveryTimer = 0f;
                                UpdatePenalties();
                            }
                        }
                        _velocityY = 0f; _isGrounded = true; _currentPlatform = platform; _playerPosition.Y = platform.Bounds.Top - _feetYOffset;
                        _ignoredPlatform = null; hitAnything = true; break;
                    }
                }
            }

            if (!hitAnything && _currentPlatform != null)
            {
                if (_playerPosition.X + 35 > _currentPlatform.Bounds.Right || _playerPosition.X + 75 < _currentPlatform.Bounds.Left)
                {
                    _isGrounded = false; _currentPlatform = null; _fallStartY = _playerPosition.Y;
                }
            }

            if (_playerPosition.Y + _feetYOffset >= _jungleFloorSurfaceY)
            {
                if (!_isGrounded)
                {
                    float fallDistance = _playerPosition.Y - _fallStartY;
                    if (fallDistance > 150f)
                    {
                        int damage = ((int)(fallDistance / 150f)) * 10;
                        _currentHp = Math.Max(10, _currentHp - damage);
                        _recoveryTimer = 0f;
                        UpdatePenalties();
                    }
                }
                _playerPosition.Y = _jungleFloorSurfaceY - _feetYOffset; _velocityY = 0f; _isGrounded = true; _currentPlatform = null;
            }

            _playerPosition.X = Math.Clamp(_playerPosition.X, 0f, 1280 - _finalWidth);
            _animationTimer += deltaTime;
            if (_animationTimer >= _animationDelay) { _animationTimer = 0f; _useAltFrame = !_useAltFrame; }
            _activeTexture = !_isGrounded ? _textureJump : (isMoving ? (_useAltFrame ? _textureRun2 : _textureRun1) : (_useAltFrame ? _textureIdle2 : _textureIdle1));

            _previousKeyboardState = kState;
            _previousMouseState = mouse;
        }

        private void UpdatePenalties()
        {
            int lostSegments = (100 - _currentHp) / 10;
            _playerSpeed = Math.Max(100f, 280f - (lostSegments * 15f));
            _jumpForce = Math.Min(-300f, -700f + (lostSegments * 40f));
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            if (_currentState == GameTitle.Title)
            {
                _menuScreen.Draw(_spriteBatch);
            }
            else if (_currentState == GameTitle.Playing)
            {
                _spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, 1280, 720), Color.White);
                foreach (var platform in _platforms) _spriteBatch.Draw(platform.Texture, platform.Bounds, Color.White);

                if (!_amuletCollected)
                {
                    _spriteBatch.Draw(_amuletTexture, new Rectangle((int)_amuletPosition.X, (int)(_amuletPosition.Y + (float)Math.Sin(_amuletFloatTimer * 3f) * 10f), 40, 40), Color.White);
                }

                _spriteBatch.Draw(_activeTexture, new Rectangle((int)_playerPosition.X, (int)_playerPosition.Y, _finalWidth, _finalHeight), null, Color.White, 0f, Vector2.Zero, _spriteFlip, 0f);
                if (_hpTextures.ContainsKey(_currentHp)) _spriteBatch.Draw(_hpTextures[_currentHp], new Vector2(20, 20), null, Color.White, 0f, Vector2.Zero, 0.1f, SpriteEffects.None, 0f);
                _spriteBatch.Draw(_bagIcon, _bagBounds, Color.White);

                if (_isInventoryOpen)
                {
                    _spriteBatch.Draw(_inventoryIcon, _inventoryBounds, Color.White);
                    if (_hasAmulet)
                    {
                        _spriteBatch.Draw(_amuletTexture, new Rectangle(560, 195, 50, 50), Color.White);
                    }
                }
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
