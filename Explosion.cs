using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Airwolf2023
{
    public class Explosion : Character
    {
        private SoundEffect _explosionSound;
        private SoundEffectInstance _explosionSoundInstance;

        public Explosion(string spriteSheetAsset, int frameWidth, int frameHeight, Game game) : base(spriteSheetAsset, frameWidth, frameHeight, game) 
        { 
            Visible = false;
            Enabled = false;
        }

        protected override void LoadContent()
        {
            _spriteSheet = new SpriteSheet(Game.Content, _spriteSheetAsset, _frameWidth, _frameHeight, _frameWidth / 2, 5);
            _spriteSheet.RegisterAnimation("Explode", 0, 5, 8f);

            _explosionSound = Game.Content.Load<SoundEffect>("pouahwouha");
            _explosionSoundInstance = _explosionSound.CreateInstance();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Animate((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        private int _playedCounter;

        public void Explode(Action onExploded = null)
        {
            Visible = true;
            Enabled = true;
            _playedCounter = 0;
            SetFrame(0);
            SetAnimation("Explode", () =>
            {
                _playedCounter++;
                if (_playedCounter == 2)
                {
                    Visible = false;
                    Enabled = false;
                    onExploded?.Invoke();
                }
            });
            _explosionSoundInstance.Play();
        }
    }
}
