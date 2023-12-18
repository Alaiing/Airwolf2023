using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public LevelSection(SpriteSheet backgroundSheet, int frame, Game game) : base(game)
        {
            _frame = frame;
            _backgroundSheet = backgroundSheet;
            DrawOrder = 10;
        }

        public void AddEnemy(Enemy enemy, Vector2 position)
        {
            _enemies.Add(enemy);
            enemy.MoveTo(position);
        }

        public void Activate()
        {
            Enabled = true;
            Visible = true;
            _backgroundSheet.RestoreOriginalTexture();
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

        public override void Draw(GameTime gameTime)
        {
            _backgroundSheet.DrawFrame(_frame, SpriteBatch, new Vector2(0, Airwolf.BACKGROUND_POSITION_Y), 0, Vector2.One, Color.White);
        }
    }
}
