using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Airwolf2023
{
    public class LevelSection : DrawableGameComponent
    {
        protected new OudidonGame Game => (base.Game as OudidonGame);
        protected SpriteBatch SpriteBatch => Game.SpriteBatch;

        private SpriteSheet _backgroundSheet;
        private int _frame;
        public int Frame => _frame;
        private readonly List<Enemy> _enemies = new List<Enemy>();
        public List<Enemy> Enemies => _enemies;
        public Gate _gate;
        public bool gateOpen;
        private readonly Dictionary<Point, int> _switches = new();

        private Vector2 _offset;
        public Vector2 Offset => _offset;

        public LevelSection(SpriteSheet backgroundSheet, int frame, Game game) : base(game)
        {
            _frame = frame;
            _backgroundSheet = backgroundSheet;
            DrawOrder = 9;
        }

        public void AddEnemy(Enemy enemy, Vector2 position)
        {
            _enemies.Add(enemy);
            enemy.MoveTo(position);
        }

        public void AddGate(Gate gate, Vector2 position)
        {
            AddEnemy(gate, position);
            _gate = gate;
        }

        public void AddSwitch(Point position, int gateSection)
        {
            _switches.Add(position, gateSection);
        }

        public int GetSwitchGate(Point position) 
        { 
            if (_switches.TryGetValue(position, out int switchGate))
            {
                return switchGate;
            }

            return -1;
        }

        public void Show()
        {
            Visible = true;
        }

        public void Hide()
        { 
            Visible = false; 
        }

        public void Activate()
        {
            Enabled = true;
            Visible = true;
            //_backgroundSheet.RestoreOriginalTexture();
            foreach (Enemy enemy in _enemies)
            {
                Game.Components.Add(enemy);
            }
        }

        public void Deactivate()
        {
            Enabled = false;
            Visible = false;
            foreach (Enemy enemy in _enemies)
            {
                Game.Components.Remove(enemy);
            }
        }

        public void Pause()
        {
            Enabled = false;
            foreach (Enemy enemy in _enemies)
            {
                enemy.Enabled = false;
            }
        }

        public void UnPause()
        {
            Enabled = false;
            foreach (Enemy enemy in _enemies)
            {
                enemy.Enabled = true;
            }
        }

        public void SetOffset(Vector2 offset)
        {
            _offset = offset;
            foreach (Enemy enemy in _enemies)
            {
                enemy.drawOffset = offset;
            }
        }

        public void Reset()
        {
            _backgroundSheet.RestoreOriginalTexture();
            _offset = Vector2.Zero;
            foreach (Enemy enemy in _enemies)
            {
                enemy.Reset();
            }
            UpdateGate();
        }

        public void UpdateGate()
        {
            if (_gate != null)
            {
                _gate.Visible = !gateOpen;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            _backgroundSheet.DrawFrame(_frame, SpriteBatch, new Vector2(0, 0) + _offset, 0, Vector2.One, Color.White);
        }
    }
}
