using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Airwolf2023
{
    public class Gate : Enemy
    {
        public Gate(string spriteSheetAsset, int frameWidth, int frameHeight, Game game) : base(spriteSheetAsset, frameWidth, frameHeight, game)
        {
        }

        protected override void LoadContent()
        {
            _spriteSheet = new SpriteSheet(Game.Content, _spriteSheetAsset, _frameWidth, _frameHeight);
            _spriteSheet.RegisterAnimation("Idle", 0, 0, 0f);
            SetAnimation("Idle");
        }

    }
}
