using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Airwolf2023
{
    public class SuperCopter : Character
    {
        private const string STATE_HORIZONTAL = "StateHorizontal";
        private const string STATE_VERTICAL = "StateVertical";
        private const string STATE_DEATH = "StateDeath";

        private const float _fallSpeed = 30f;
        private const float _verticalVelocity = 50f;
        private const float _horizontalVelocity = 50f;
        private const float _fallAccelerationDuration = 0.5f;

        private float _verticalSpeed;
        private float _horizontalSpeed;
        private float _fallAcceleration;

        private bool _wasGoingUp;
        private bool _wasGoingDown;

        private float _horizontalDirection;

        private SimpleStateMachine _stateMachine;

        private int _armour;
        public int Armour => _armour;
        private float _damageCooldown;
        private bool _isColliding;

        public bool IsHorizontal => _stateMachine.CurrentState == STATE_HORIZONTAL;
        public bool IsVertical => !IsHorizontal;
        public bool IsExploding => _stateMachine.CurrentState == STATE_DEATH;

        private SoundEffect _flapSound;
        private SoundEffectInstance _flapSoundInstance;
        private SoundEffect _collisionSound;
        private SoundEffectInstance _collisionSoundInstance;

        private SpriteSheet _helixSheet;
        private float _helixFrame;

        public SuperCopter(string spriteSheet, int frameWidth, int frameHeight, Game game) : base(spriteSheet, frameWidth, frameHeight, game) { }

        protected override void LoadContent()
        {
            _spriteSheet = new SpriteSheet(Game.Content, _spriteSheetAsset, 27, 13, 13, -2);
            _spriteSheet.RegisterAnimation("Horizontal", 0, 0, 1f);
            _spriteSheet.RegisterAnimation("ToVertical", 1, 3, 8f);
            _spriteSheet.RegisterAnimation("ToHorizontal", 2, 1, 8f);
            _spriteSheet.RegisterAnimation("Vertical", 3, 3, 1f);

            _helixSheet = new SpriteSheet(Game.Content, "helix", 24, 1, 12, 0);

            _flapSound = Game.Content.Load<SoundEffect>("blblbl");
            _flapSoundInstance = _flapSound.CreateInstance();
            _flapSoundInstance.IsLooped = true;
            _flapSoundInstance.Volume = 0.5f;
            _flapSoundInstance.Pan = 0f;

            _collisionSound = Game.Content.Load<SoundEffect>("co");
            _collisionSoundInstance = _collisionSound.CreateInstance();
            _collisionSoundInstance.Volume = 0.5f;
        }

        public override void Initialize()
        {
            base.Initialize();

            _stateMachine = new SimpleStateMachine();
            _stateMachine.AddState(STATE_HORIZONTAL, null, null, OnUpdate: BaseUpdate, null);
            _stateMachine.AddState(STATE_VERTICAL, VerticalEnter, VerticalExit, OnUpdate: BaseUpdate, null);
            _stateMachine.AddState(STATE_DEATH, null, null, null, null);
            _stateMachine.SetState(STATE_HORIZONTAL);

            _fallAcceleration = _fallSpeed / _fallAccelerationDuration;

            Reset();
        }

        public void StartSound()
        {
            _flapSoundInstance.Play();
        }

        public void StopSound()
        {
            _flapSoundInstance.Stop();
        }

        public void Reset()
        {
            _armour = 6;
            _verticalSpeed = 0f;
            _horizontalSpeed = 0f;
            _horizontalDirection = -1;
            MoveTo(new Vector2(109, 55));
            LookTo(new Vector2(_horizontalDirection, 0));
            _stateMachine.SetState(STATE_HORIZONTAL);
        }

        public void Collides(Vector2 relativeContactPosition)
        {
            _collisionSoundInstance.Play();
            if (_damageCooldown >= 0.25f)
            {
                _damageCooldown = 0;
                if (!Airwolf.CheatNoDamage)
                {
                    _armour--;
                }
            }
            int pushBackX = -Math.Sign(relativeContactPosition.X);
            int pushBackY = -Math.Sign(relativeContactPosition.Y); // (_wasGoingDown || _wasGoingUp || _horizontalSpeed == 0) ? -Math.Sign(_verticalSpeed) : 0;
            MoveBy(new Vector2(pushBackX, pushBackY) * 2);
            _verticalSpeed = 0f;
            _isColliding = true;
        }

        public new void Die()
        {
            StopSound();
            _stateMachine.SetState(STATE_DEATH);
        }

        public void StopColliding()
        {
            _isColliding = false;
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _stateMachine.Update(gameTime);
            if (!_isColliding)
            {
                Animate(deltaTime);
            }
            _helixFrame += 40 * deltaTime;
            if (_helixFrame >= _helixSheet.FrameCount) 
            { 
                _helixFrame = 0; 
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            _helixSheet.DrawFrame((int)MathF.Floor(_helixFrame), SpriteBatch, Position + drawOffset, 0, Vector2.One, Color.White);
        }

        private void BaseUpdate(GameTime gameTime, float stateTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _damageCooldown += deltaTime;

            SimpleControls.GetStates();

            if (SimpleControls.IsUpDown(PlayerIndex.One))
            {
                _verticalSpeed = -_verticalVelocity;
                _wasGoingUp = true;
            }
            else if (SimpleControls.IsDownDown(PlayerIndex.One))
            {
                _verticalSpeed = _verticalVelocity;
                _wasGoingDown = true;
            }
            else
            {
                if (_wasGoingUp)
                {
                    _verticalSpeed = 0;
                    _wasGoingUp = false;
                }
                else if (_wasGoingDown)
                {
                    _verticalSpeed = _fallSpeed;
                    _wasGoingDown = false;
                    if(Airwolf.CheatNoGravity)
                    {
                        _verticalSpeed = 0;
                    }
                }
                if (!_isColliding && !Airwolf.CheatNoGravity)
                {
                    _verticalSpeed = MathF.Min(_verticalSpeed + deltaTime * _fallAcceleration, _fallSpeed);
                }
            }

            if (SimpleControls.IsLeftDown(PlayerIndex.One))
            {
                _horizontalSpeed = -_horizontalVelocity;
                if (_stateMachine.CurrentState == STATE_VERTICAL && _verticalAnimationDone)
                {
                    _horizontalDirection = -1;
                    _stateMachine.SetState(STATE_HORIZONTAL);
                }
                else
                {
                    if (_horizontalDirection > 0)
                    {
                        _stateMachine.SetState(STATE_VERTICAL);
                    }
                    _horizontalDirection = -1;
                }
            }
            else if (SimpleControls.IsRightDown(PlayerIndex.One))
            {
                _horizontalSpeed = _horizontalVelocity;
                if (_stateMachine.CurrentState == STATE_VERTICAL && _verticalAnimationDone)
                {
                    _horizontalDirection = 1;
                    _stateMachine.SetState(STATE_HORIZONTAL);
                }
                else
                {
                    if (_horizontalDirection < 0)
                    {
                        _stateMachine.SetState(STATE_VERTICAL);
                    }
                    _horizontalDirection = 1;
                }
            }
            else
            {
                _horizontalSpeed = 0;
            }

            if (SimpleControls.IsADown(PlayerIndex.One)) // TODO: fire if pressed this frame
            {
                EventsManager.FireEvent("FIRE", this);
            }

            MoveBy(new Vector2(deltaTime * _horizontalSpeed, deltaTime * _verticalSpeed));
        }

        private bool _verticalAnimationDone;

        private void VerticalExit()
        {
            _verticalAnimationDone = false;
            LookTo(new Vector2(_horizontalDirection, 0));
            SetAnimation("ToHorizontal", () =>
            {
                SetAnimation("Horizontal");
                _verticalAnimationDone = true;
            });
        }

        private void VerticalEnter()
        {
            _verticalAnimationDone = false;
            SetAnimation("ToVertical", () =>
            {
                SetAnimation("Vertical");
                _verticalAnimationDone = true;
            });
        }
    }
}
