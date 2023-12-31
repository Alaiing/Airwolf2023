﻿using C3.MonoGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Policy;

namespace Airwolf2023
{
    public class Airwolf : OudidonGame
    {
        private const string STATE_TITLE = "StateTitle";
        private const string STATE_GAME = "StateGame";

        public const int BACKGROUND_POSITION_Y = 64;
        private static readonly Color backgroundColor = new Color(0.498f, 0.498f, 1f);
        private static readonly Color destructibleBackgroundColor = new Color(127, 255, 255, 255);

        private SuperCopter _superCopter;
        private Explosion _explosion;
        private Bullet _bullet;

        private SpriteSheet _backgroundsSheet;
        private int _currentLevelIndex;

        private Texture2D _armourTexture;
        private SoundEffect _wallDestructionSound;
        private SoundEffectInstance _wallDestructionSoundInstance;
        private Level _level;

        private bool _isScrolling;
        private Vector2 _scrollingDirection;
        private float _scrollingTime;
        private float _scrollingDuration = 2f;
        private int _scrollingDestinationSection;
        private Vector2 _scrollingDestinationPosition;

        private int _repositionningY;

        public static bool CheatNoCollision = false;
        public static bool CheatNoGravity = false;
        public static bool CheatNoDamage = true;
        public static bool CheatNoTimer = false;
        private int _startingSection = 0;

        private int _score;
        private int _highscore;
        private float _timer;

        private Texture2D _hud;

        private SoundEffect _mainMusic;
        private SoundEffectInstance _mainMusicInstance;

        public Airwolf(int screenWith, int screenHeight, int screenScaleX, int screenScaleY) : base(screenWith, screenHeight, screenScaleX, screenScaleY) { }

        protected override void Initialize()
        {
            _superCopter = new SuperCopter("supercopter", 27, 13, this);
            _superCopter.Initialize();
            _superCopter.SetAnimation("Horizontal");
            Components.Add(_superCopter);
            _superCopter.DrawOrder = 0;
            _repositionningY = 111 - 8 - _superCopter.SpriteSheet.BottomMargin;

            _explosion = new Explosion("explosion", 31, 24, this);
            _explosion.Initialize();
            Components.Add(_explosion);
            _superCopter.DrawOrder = 1;

            _bullet = new Bullet(this);
            Components.Add(_bullet);

            _hud = Content.Load<Texture2D>("hud");

            EventsManager.ListenTo<SuperCopter>("FIRE", OnFired);

            base.Initialize();
        }

        protected override void InitStateMachine()
        {
            AddSate(STATE_TITLE, onEnter: TitleEnter, onDraw: TitleDraw, onUpdate: TitleUpdate);
            AddSate(STATE_GAME, onEnter: GameEnter, onUpdate: GameUpdate, onDraw: GameDraw);
            SetState(STATE_TITLE);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            CreateSpriteFont();

            _backgroundsSheet = new SpriteSheet(Content, "backgrounds", 160, 111);

            _armourTexture = Content.Load<Texture2D>("armour");

            _wallDestructionSound = Content.Load<SoundEffect>("coin");
            _wallDestructionSoundInstance = _wallDestructionSound.CreateInstance();
            _wallDestructionSoundInstance.Volume = 0.5f;

            _level = Level.CreateLevel(_backgroundsSheet, this);

            _mainMusic = Content.Load<SoundEffect>("airwolf");
            _mainMusicInstance = _mainMusic.CreateInstance();
            _mainMusicInstance.IsLooped = true;

            base.LoadContent();
        }

        private void StartGame()
        {
            SetState(STATE_GAME);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.Clear(new Color(0.498f, 0f, 0.498f));

            GraphicsDevice.Viewport = new Viewport(0, BACKGROUND_POSITION_Y, _screenWidth, 111);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);

            StateMachineDraw(_spriteBatch, (int)gameTime.ElapsedGameTime.TotalSeconds);
            base.Draw(gameTime);
            //if (_stateMachine.CurrentState == STATE_GAME)
            //{
            //    DrawLevelMask();
            //}
            _spriteBatch.End();

            GraphicsDevice.Viewport = new Viewport(0, 0, _screenWidth, _screenHeight);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            DrawHUD();
            DrawArmour();
            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            _spriteBatch.Draw(_renderTarget, new Rectangle((int)MathF.Floor(CameraShake.ShakeOffset.X), (int)MathF.Floor(CameraShake.ShakeOffset.Y), _screenWidth * _screenScaleX, _screenHeight * _screenScaleY), Color.White);
            _spriteBatch.End();
        }

        List<Vector2> _contacts = new List<Vector2>();

        private bool TestContact(out Vector2 relativeContactPoint)
        {
            _contacts.Clear();
            for (int x = 0; x < _superCopter.SpriteSheet.FrameWidth; x++)
                for (int y = -1; y < _superCopter.SpriteSheet.FrameHeight; y++)
                {
                    if (y < 0 || _superCopter.GetPixel(x, y).A > 0)
                    {
                        int backgroundX = _superCopter.PixelPositionX - _superCopter.SpriteSheet.LeftMargin + x;
                        int backgroundY = _superCopter.PixelPositionY - _superCopter.SpriteSheet.TopMargin + y;

                        Color backgroundPixel = GetBackgroundColor(backgroundX, backgroundY);
                        if (backgroundPixel.A > 0)
                        {
                            _contacts.Add(new Vector2(x - _superCopter.SpriteSheet.FrameWidth / 2, y - _superCopter.SpriteSheet.FrameHeight / 2));
                        }

                        foreach (Enemy enemy in _level.CurrentEnemies)
                        {
                            if (enemy.Visible)
                            {
                                Color enemyPixel = enemy.GetPixel(backgroundX - enemy.PixelPositionX, backgroundY - enemy.PixelPositionY);
                                if (enemyPixel.A > 0)
                                {
                                    _contacts.Add(new Vector2(x - _superCopter.SpriteSheet.FrameWidth / 2, y - _superCopter.SpriteSheet.FrameHeight / 2));
                                    //return true;
                                }
                            }
                        }
                    }
                }

            relativeContactPoint = new Vector2();
            if (_contacts.Count > 0) 
            {
                for (int i  =  0; i < _contacts.Count;i++)
                {
                    relativeContactPoint += _contacts[i];
                }
                relativeContactPoint /= _contacts.Count;
                return true; 
            }

            return false;
        }

        private Color GetBackgroundColor(int backgroundX, int backgroundY)
        {
            return _backgroundsSheet.GetPixel(_level.CurrentBackgroundFrame, backgroundX, backgroundY);
        }

        private bool TestBulletContact()
        {
            Point positionInBackground = new Point(_bullet.PixelPosition.X, _bullet.PixelPosition.Y);
            positionInBackground.Y = (positionInBackground.Y / 2) * 2;
            Color _bulletPreviousBackgroundColor = GetBackgroundColor(positionInBackground.X - _bullet.DirectionX, positionInBackground.Y - _bullet.DirectionY);
            Color _bulletBackgroundColor = GetBackgroundColor(positionInBackground.X, positionInBackground.Y);


            if (_bulletBackgroundColor == destructibleBackgroundColor)
            {
                if (_bulletPreviousBackgroundColor == destructibleBackgroundColor)
                {
                    positionInBackground.X -= _bullet.DirectionX;
                }
                _backgroundsSheet.SetPixel(_level.CurrentBackgroundFrame, positionInBackground.X, positionInBackground.Y, new Color(0, 0, 0, 0));
                _backgroundsSheet.SetPixel(_level.CurrentBackgroundFrame, positionInBackground.X, positionInBackground.Y + 1, new Color(0, 0, 0, 0));
                _backgroundsSheet.SetPixel(_level.CurrentBackgroundFrame, positionInBackground.X + _bullet.DirectionX, positionInBackground.Y, new Color(0, 0, 0, 0));
                _backgroundsSheet.SetPixel(_level.CurrentBackgroundFrame, positionInBackground.X + _bullet.DirectionX, positionInBackground.Y + 1, new Color(0, 0, 0, 0));

                int gateSection = _level.GetSwitchGateApprox(new Point(positionInBackground.X, positionInBackground.Y), _level.CurrentSectionIndex);
                if (gateSection >= 0)
                {
                    _level.GetSection(gateSection).gateOpen = true;
                }

                _wallDestructionSoundInstance.Play();
                _score += 2;
            }

            if (_bulletBackgroundColor.A > 0)
                return true;

            foreach (Enemy enemy in _level.CurrentEnemies)
            {
                Color enemyPixel = enemy.GetPixel(positionInBackground.X - enemy.PixelPositionX, positionInBackground.Y - enemy.PixelPositionY);
                if (enemyPixel.A > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnFired(SuperCopter superCopter)
        {
            if (!_bullet.Visible)
            {
                Vector2 direction;
                if (superCopter.IsHorizontal)
                {
                    direction = new(Math.Sign(superCopter.MoveDirection.X), 0);
                }
                else
                {
                    direction = new(0, 1);
                }
                Vector2 offset = new Vector2(13 * direction.X, 13);
                _bullet.Fire(superCopter.Position + offset, direction);
            }
        }

        private void DrawArmour()
        {
            Vector2 position = new Vector2(44, 116 + BACKGROUND_POSITION_Y);
            for (int i = 0; i < 6; i++)
            {
                if (_superCopter.Armour >= 6 - i && _stateMachine.CurrentState == STATE_GAME)
                {
                    _spriteBatch.Draw(_armourTexture, position, Color.White);
                }
                else
                {
                    _spriteBatch.FillRectangle(position, new Vector2(8, 16), Color.Blue);
                }
                position.X += 12;
            }
        }

        private void DrawHUD()
        {
            _spriteBatch.Draw(_hud, new Vector2(1,1), Color.White);
            _spriteBatch.DrawString(_spriteFont, _highscore.ToString("00000"), new Vector2(16, 25), Color.White);
            _spriteBatch.DrawString(_spriteFont, _score.ToString("00000"), new Vector2(104, 25), Color.White);
            int seconds = (int)MathF.Floor(_timer);
            _spriteBatch.DrawString(_spriteFont, seconds.ToString("00"), new Vector2(62, 41), Color.White);
            int hundredth = (int)MathF.Floor((_timer - (int)MathF.Floor(_timer)) * 100);
            _spriteBatch.DrawString(_spriteFont, hundredth.ToString("00"), new Vector2(82, 41), Color.White);
        }

        SpriteFont _spriteFont;
        private void CreateSpriteFont()
        {
            Texture2D fontTexture = Content.Load<Texture2D>("digits");
            List<Rectangle> glyphBounds = new List<Rectangle>();
            List<Rectangle> cropping = new List<Rectangle>();
            string charsString = "0123456789";
            List<char> chars = new List<char>();
            chars.AddRange(charsString);
            List<Vector3> kerning = new List<Vector3>();
            for (int i = 0; i < fontTexture.Width / 8; i++)
            {
                glyphBounds.Add(new Rectangle(i * 8, 0, 8, 15));
                cropping.Add(new Rectangle(0, 0, 8, 15));
                kerning.Add(new Vector3(0, 8, 0));
            }
            _spriteFont = new SpriteFont(fontTexture, glyphBounds, cropping, chars, 0, 0, kerning, '0');
        }


        #region States
        private void GameUpdate(GameTime gameTime, float stateTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_isScrolling)
            {
                _scrollingTime += deltaTime;
                _level.GetSection(_scrollingDestinationSection).SetOffset(Vector2.Lerp(_scrollingDestinationPosition, Vector2.Zero, _scrollingTime / _scrollingDuration));
                _level.GetSection(_level.CurrentSectionIndex).SetOffset(Vector2.Lerp(Vector2.Zero, -_scrollingDestinationPosition, _scrollingTime / _scrollingDuration));
                _superCopter.drawOffset = _level.GetSection(_level.CurrentSectionIndex).Offset;
                if (_scrollingTime >= _scrollingDuration)
                {
                    EndScrolling();
                }
            }
            else
            {
                if (!_superCopter.IsExploding)
                {
                    if (!CheatNoTimer)
                    {
                        _timer = MathF.Max(0, _timer - deltaTime);
                    }
                    if (_timer <= 0)
                    {
                        Die();
                        return;
                    }
                }

                if (!CheatNoCollision && TestContact(out Vector2 relativeContactPoint))
                {
                    _superCopter.Collides(relativeContactPoint);
                    if (_superCopter.Armour < 0 && !_explosion.Visible)
                    {
                        Die();
                    }
                }
                else
                {
                    _superCopter.StopColliding();
                }

                if (_bullet.Visible && TestBulletContact())
                {
                    _bullet.Remove();
                }

                if (_superCopter.PixelPositionY > _repositionningY)
                {
                    ChangeSection(_level.CurrentSectionIndex + 4, new Vector2(0, 1));
                } 
                else if (_superCopter.PixelPositionY < 0)
                {
                    ChangeSection(_level.CurrentSectionIndex - 4, new Vector2(0, -1));
                }

                if (_superCopter.PixelPositionX - _superCopter.SpriteSheet.LeftMargin < 4)
                {
                    ChangeSection(_level.CurrentSectionIndex - 1, new Vector2(-1, 0));
                }
                else if (_superCopter.PixelPositionX + _superCopter.SpriteSheet.RightMargin > _screenWidth - 4)
                {
                    ChangeSection(_level.CurrentSectionIndex + 1, new Vector2(1, 0));
                }
            }
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.S))
            {
                _mainMusicInstance.Stop();
            }
        }

        private void Die()
        {
            _superCopter.Die();
            _explosion.MoveTo(_superCopter.Position);
            _explosion.Explode(() => SetState(STATE_TITLE));
        }

        private void ChangeSection(int nextSection, Vector2 direction)
        {
            // Tout pauser
            _superCopter.Enabled = false;
            _level.GetSection(_level.CurrentSectionIndex).Pause();

            // Scroller vers la section suivante
            _level.GetSection(nextSection).Show();
            _level.GetSection(nextSection).UpdateGate();

            _isScrolling = true;
            _scrollingDestinationSection = nextSection;
            _scrollingTime = 0;
            _scrollingDirection = direction;
            _scrollingDestinationPosition = new Vector2(direction.X * _screenWidth, direction.Y * 111);
            _level.GetSection(_scrollingDestinationSection).SetOffset(_scrollingDestinationPosition);
        }

        private void EndScrolling()
        {
            _isScrolling = false;
            _level.GetSection(_scrollingDestinationSection).SetOffset(Vector2.Zero);
            _level.GetSection(_level.CurrentSectionIndex).SetOffset(Vector2.Zero);
            _superCopter.drawOffset = Vector2.Zero;

            _level.GetSection(_level.CurrentSectionIndex).Reset();
            _level.GetSection(_level.CurrentSectionIndex).Deactivate();
            _level.SetCurrentSection(_scrollingDestinationSection);

            // Replacer l'hélico
            Vector2 newPosition = new Vector2(_superCopter.PixelPositionX, _superCopter.PixelPositionY);
            newPosition.Y += -_scrollingDirection.Y * _repositionningY;
            if (_scrollingDirection.X > 0)
            {
                newPosition.X = 4 + _superCopter.SpriteSheet.LeftMargin;
            }
            else if (_scrollingDirection.X < 0)
            {
                newPosition.X = _screenWidth - 4 -  _superCopter.SpriteSheet.RightMargin;
            }
            _superCopter.MoveTo(newPosition);

            // Tout réactiver
            _superCopter.Enabled = true;
        }

        private void GameDraw(SpriteBatch batch, float deltaTime)
        {
            _spriteBatch.FillRectangle(0, 0, 160, 111, backgroundColor);
        }

        private void GameEnter()
        {
            _superCopter.Reset();
            _superCopter.Visible = true;
            _currentLevelIndex = 0;
            _score = 0;
            _timer = 60f;
            _level.Activate(this);
            _level.SetCurrentSection(_startingSection);
            _level.GetSection(_startingSection).Reset();
            _level.Reset();
            _superCopter.StartSound();
            _mainMusicInstance.Play();
        }

        private void TitleEnter()
        {
            if (_score > _highscore)
            {
                _highscore = _score;
            }
            _level.Deactivate(this);
            _superCopter.Visible = false;
        }

        private void TitleUpdate(GameTime time, float arg2)
        {
            SimpleControls.GetStates();
            if (SimpleControls.IsADown(PlayerIndex.One) )
                StartGame();
        }

        private void TitleDraw(SpriteBatch batch, float deltaTime)
        {
            _spriteBatch.FillRectangle(0, 0, 160, 111, backgroundColor);
            _backgroundsSheet.DrawFrame(1, batch, new Vector2(0, 0), 0, Vector2.One, Color.White);
        }

        #endregion

        #region Debug
        private void DrawContacts()
        {
            foreach (Vector2 contact in _contacts)
            {
                _spriteBatch.DrawLine(contact + new Vector2(0, 0), contact + new Vector2(1, 0), Color.LightYellow);
            }
        }

        private void DrawLevelMask()
        {
            for (int x = 0; x < _backgroundsSheet.FrameWidth; x++)
                for (int y = 0; y < _backgroundsSheet.FrameHeight; y++)
                {
                    Color backgroundPixel = GetBackgroundColor(x, y);
                    if (backgroundPixel.A > 0)
                    {
                        _spriteBatch.DrawRectangle(new Vector2(x, y), Vector2.One, Color.Green);
                    }
                }
            if (_level.CurrentEnemies != null)
            {
                foreach (Enemy enemy in _level.CurrentEnemies)
                {
                    if (enemy.Visible)
                    {
                        for (int x = 0; x < enemy.SpriteSheet.FrameWidth; x++)
                            for (int y = 0; y < enemy.SpriteSheet.FrameHeight; y++)
                            {
                                Color backgroundPixel = enemy.GetPixel(x, y);
                                if (backgroundPixel.A > 0)
                                {
                                    _spriteBatch.DrawRectangle(new Vector2(x + enemy.PixelPositionX, y + enemy.PixelPositionY), Vector2.One, Color.Green);
                                }
                            }
                    }
                }
            }
        }
        #endregion
    }
}
