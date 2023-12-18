using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oudidon
{
    public class OudidonGame : Game
    {
        protected int _screenWidth;
        public int ScreenWidth => _screenWidth;
        protected int _screenHeight;
        public int ScreenHeight => _screenHeight;
        protected int _screenScaleX;
        protected int _screenScaleY;

        protected GraphicsDeviceManager _graphics;
        protected SpriteBatch _spriteBatch;
        public SpriteBatch SpriteBatch => _spriteBatch;

        protected RenderTarget2D _renderTarget;

        protected SimpleStateMachine _stateMachine;
        protected float _deltaTime;

        public OudidonGame(int screenWith, int screenHeight, int screenScaleX, int screenScaleY) : base()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _screenWidth = screenWith;
            _screenHeight = screenHeight;
            _screenScaleX = screenScaleX;
            _screenScaleY = screenScaleY;
            _graphics.PreferredBackBufferWidth = _screenWidth * _screenScaleX;
            _graphics.PreferredBackBufferHeight = _screenHeight * _screenScaleY;
            _graphics.ApplyChanges();

            CommonRandom.Init();
        }

        protected override void Initialize()
        {
            base.Initialize();
            _stateMachine = new SimpleStateMachine();
            InitStateMachine();
            _renderTarget = new RenderTarget2D(GraphicsDevice, _screenWidth, _screenHeight);
        }

        protected override void Update(GameTime gameTime)
        {
            _deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _stateMachine.Update(gameTime);
            base.Update(gameTime);
        }

        protected virtual void InitStateMachine() { }

        protected void StateMachineDraw(SpriteBatch spriteBatch, float deltaTime)
        {
            _stateMachine.Draw(spriteBatch, deltaTime);
        }

        protected void AddSate(string name, Action onEnter = null, Action onExit = null, Action<GameTime, float> onUpdate = null, Action<SpriteBatch, float> onDraw = null)
        {
            _stateMachine.AddState(name, onEnter, onExit, onUpdate, onDraw);
        }

        protected void SetState(string state)
        {
            _stateMachine.SetState(state);
        }
    }
}
