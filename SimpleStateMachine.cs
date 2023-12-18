using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Oudidon
{
    public class SimpleStateMachine
    {
        public class State
        {
            public string name;
            public Action OnEnter;
            public Action<GameTime, float> OnUpdate;
            public Action OnExit;
            public Action<SpriteBatch, float> OnDraw;
            public float timer;
        }

        private readonly Dictionary<string, State> _states = new Dictionary<string, State>();

        private State _currentState;
        public string CurrentState => _currentState?.name;
        public float CurrentStateTimer => _currentState.timer;

        public void AddState(string name, Action OnEnter, Action OnExit, Action<GameTime, float> OnUpdate, Action<SpriteBatch, float> OnDraw)
        {
            if (!_states.ContainsKey(name))
            {
                _states.Add(name, new State { name = name, OnEnter = OnEnter, OnExit = OnExit, OnUpdate = OnUpdate, OnDraw = OnDraw });
            }
        }

        public void SetState(string name)
        {
            _currentState?.OnExit?.Invoke();
            if (_states.TryGetValue(name, out _currentState))
            {
                _currentState.timer = 0;
                _currentState.OnEnter?.Invoke();
            }
        }

        public void Update(GameTime gameTime)
        {
            _currentState.timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            _currentState?.OnUpdate?.Invoke(gameTime, _currentState.timer);
        }

        public void Draw(SpriteBatch spriteBatch, float deltaTime)
        {
            _currentState?.OnDraw?.Invoke(spriteBatch, deltaTime);
        }
    }
}
