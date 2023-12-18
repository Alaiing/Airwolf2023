using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Airwolf2023
{
    public class Enemy : Character
    {
        public Enemy(string spriteSheetAsset, Game game) : base(spriteSheetAsset, game)
        {
            DrawOrder = 9;    
        }

        protected override void LoadContent()
        {
            _spriteSheet = new SpriteSheet(Game.Content, _spriteSheetAsset, 8, 16);
            _spriteSheet.RegisterAnimation("Idle", 0, 3, 8f);
            SetAnimation("Idle");
        }

        public override void Update(GameTime gameTime)
        {
            Animate((float)gameTime.ElapsedGameTime.TotalSeconds);
        }
    }
}
