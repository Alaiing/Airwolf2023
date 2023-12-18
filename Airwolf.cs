using C3.MonoGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
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

        private Level _level;

        public Airwolf(int screenWith, int screenHeight, int screenScaleX, int screenScaleY) : base(screenWith, screenHeight, screenScaleX, screenScaleY) { }

        protected override void Initialize()
        {
            _superCopter = new SuperCopter("supercopter", this);
            _superCopter.Initialize();
            _superCopter.SetAnimation("Horizontal");
            Components.Add(_superCopter);
            _superCopter.DrawOrder = 0;

            _explosion = new Explosion("explosion", this);
            _explosion.Initialize();
            Components.Add(_explosion);
            _superCopter.DrawOrder = 1;

            _bullet = new Bullet(this);
            Components.Add(_bullet);


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

            _backgroundsSheet = new SpriteSheet(Content, "backgrounds", 160, 111);

            _armourTexture = Content.Load<Texture2D>("armour");

            _level = Level.CreateLevel(_backgroundsSheet, this);

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
            // TODO: Add your drawing code here
            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.Clear(new Color(0.498f, 0f, 0.498f));
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);


            StateMachineDraw(_spriteBatch, (int)gameTime.ElapsedGameTime.TotalSeconds);
            base.Draw(gameTime);

            DrawArmour();

            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            _spriteBatch.Draw(_renderTarget, new Rectangle((int)MathF.Floor(CameraShake.ShakeOffset.X), (int)MathF.Floor(CameraShake.ShakeOffset.Y), _screenWidth * _screenScaleX, _screenHeight * _screenScaleY), Color.White);
            _spriteBatch.End();
        }

        List<Vector2> debugContacts = new List<Vector2>();
        private bool TestContact(out Point relativeContactPoint)
        {
            debugContacts.Clear();
            for (int x = 0; x < _superCopter.SpriteSheet.FrameWidth; x++)
                for (int y = 0; y < _superCopter.SpriteSheet.FrameHeight; y++)
                {
                    Color pixel = _superCopter.GetPixel(x, y);
                    if (pixel.A > 0)
                    {
                        int backgroundX = _superCopter.PixelPositionX - _superCopter.SpriteSheet.LeftMargin + x;
                        int backgroundY = (_superCopter.PixelPositionY - _superCopter.SpriteSheet.TopMargin - BACKGROUND_POSITION_Y) + y;

                        Color backgroundPixel = GetBackgroundColor(backgroundX, backgroundY);
                        if (backgroundPixel.A > 0)
                        {
                            //debugContacts.Add(new Vector2(backgroundX, backgroundY));
                            relativeContactPoint = new Point(x - _superCopter.SpriteSheet.FrameWidth / 2, y - _superCopter.SpriteSheet.FrameHeight / 2);
                            return true;
                        }

                        foreach (Enemy enemy in _level.CurrentEnemies)
                        {
                            Color enemyPixel = enemy.GetPixel(backgroundX - enemy.PixelPositionX, backgroundY - enemy.PixelPositionY + BACKGROUND_POSITION_Y);
                            if (enemyPixel.A > 0)
                            {
                                //  debugContacts.Add(new Vector2(backgroundX, backgroundY));
                                relativeContactPoint = new Point(x - _superCopter.SpriteSheet.FrameWidth / 2, y - _superCopter.SpriteSheet.FrameHeight / 2);
                                return true;
                            }
                        }
                    }
                }

            relativeContactPoint = new Point();
            if (debugContacts.Count > 0) { return true; }
            return false;
        }

        private Color GetBackgroundColor(int backgroundX, int backgroundY)
        {
            return _backgroundsSheet.GetPixel(_currentLevelIndex, backgroundX, backgroundY);
        }

        private bool TestBulletContact()
        {
            Point positionInBackground = new Point(_bullet.PixelPosition.X, _bullet.PixelPosition.Y - BACKGROUND_POSITION_Y);
            positionInBackground.Y = (positionInBackground.Y / 2) * 2;
            Color _bulletPreviousBackgroundColor = GetBackgroundColor(positionInBackground.X - _bullet.DirectionX, positionInBackground.Y - _bullet.DirectionY);
            Color _bulletBackgroundColor = GetBackgroundColor(positionInBackground.X, positionInBackground.Y);


            if (_bulletBackgroundColor == destructibleBackgroundColor)
            {
                if (_bulletPreviousBackgroundColor == destructibleBackgroundColor)
                {
                    positionInBackground.X -= _bullet.DirectionX;
                }
                _backgroundsSheet.SetPixel(_currentLevelIndex, positionInBackground.X, positionInBackground.Y, new Color(0, 0, 0, 0));
                _backgroundsSheet.SetPixel(_currentLevelIndex, positionInBackground.X, positionInBackground.Y + 1, new Color(0, 0, 0, 0));
                _backgroundsSheet.SetPixel(_currentLevelIndex, positionInBackground.X + _bullet.DirectionX, positionInBackground.Y, new Color(0, 0, 0, 0));
                _backgroundsSheet.SetPixel(_currentLevelIndex, positionInBackground.X + _bullet.DirectionX, positionInBackground.Y + 1, new Color(0, 0, 0, 0));
            }

            return _bulletBackgroundColor.A > 0;
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

        #region States
        private void GameUpdate(GameTime gameTime, float stateTime)
        {
            if (TestContact(out Point relativeContactPoint))
            {
                _superCopter.Collides(relativeContactPoint);
                if (_superCopter.Armour < 0 && !_explosion.Visible)
                {
                    _explosion.MoveTo(_superCopter.Position);
                    _explosion.Explode(() => SetState(STATE_TITLE));
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
        }

        private void GameDraw(SpriteBatch batch, float deltaTime)
        {
            _spriteBatch.FillRectangle(0, BACKGROUND_POSITION_Y, 160, 111, backgroundColor);
        }

        private void GameEnter()
        {
            _superCopter.Reset();
            _superCopter.Visible = true;
            _currentLevelIndex = 0;
            _level.Activate(this);
            _level.SetCurrentSection(0);
        }

        private void TitleEnter()
        {
            _level.Deactivate(this);
            _superCopter.Visible = false;
        }

        private void TitleUpdate(GameTime time, float arg2)
        {
            SimpleControls.GetStates();
            if (SimpleControls.IsADown(SimpleControls.PlayerNumber.Player1))
                StartGame();
        }

        private void TitleDraw(SpriteBatch batch, float deltaTime)
        {
            _spriteBatch.FillRectangle(0, BACKGROUND_POSITION_Y, 160, 111, backgroundColor);
            _backgroundsSheet.DrawFrame(1, batch, new Vector2(0, BACKGROUND_POSITION_Y), 0, Vector2.One, Color.White);
        }

        #endregion

        #region Debug
        private void DrawContacts()
        {
            foreach (Vector2 contact in debugContacts)
            {
                _spriteBatch.DrawLine(contact + new Vector2(0, BACKGROUND_POSITION_Y), contact + new Vector2(1, BACKGROUND_POSITION_Y), Color.LightYellow);
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
                        _spriteBatch.DrawRectangle(new Vector2(x, y + BACKGROUND_POSITION_Y), Vector2.One, Color.Green);
                    }
                }
            if (_level.CurrentEnemies != null)
            {
                foreach (Enemy enemy in _level.CurrentEnemies)
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
        #endregion
    }
}
