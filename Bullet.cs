using C3.MonoGame;
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
    public class Bullet : DrawableGameComponent
    {
        protected new OudidonGame Game => (base.Game as OudidonGame);
        protected SpriteBatch SpriteBatch => Game.SpriteBatch;

        private Vector2 _position;
        private Vector2 _direction;
        public int DirectionX => (int)_direction.X;
        public int DirectionY => (int)_direction.Y;

        public Point PixelPosition => new Point((int)MathF.Floor(_position.X), (int)MathF.Floor(_position.Y));

        public Bullet(Game game) : base(game)
        {
            Remove();
        }

        public void Fire(Vector2 position, Vector2 direction)
        {
            Visible= true;
            Enabled= true;
            _position = position;
            _direction = direction;
            
        }

        public void Remove()
        {
            Visible = false;
            Enabled = false;
        }

        public override void Update(GameTime gameTime)
        {
            _position += _direction * 100f * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_position.X < 0 || _position.X > Game.ScreenWidth || _position.Y < Airwolf.BACKGROUND_POSITION_Y || _position.Y > Airwolf.BACKGROUND_POSITION_Y + 111)
            {
                Remove();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch.FillRectangle(_position, Vector2.One * 2, Color.White);
        }
    }
}
