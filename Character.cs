using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Oudidon
{
    public class Character : DrawableGameComponent
    {
        protected SpriteBatch SpriteBatch => (Game as OudidonGame).SpriteBatch;

        public string name;
        private bool _enabled = true;
        public bool IsAlive => _enabled;
        protected string _spriteSheetAsset;
        protected SpriteSheet _spriteSheet;
        public SpriteSheet SpriteSheet => _spriteSheet;
        protected Vector2 _position;
        public Vector2 Position => _position;
        public int PixelPositionX => (int)MathF.Floor(_position.X);
        public int PixelPositionY => (int)MathF.Floor(_position.Y);
        private float _currentFrame;
        public virtual int CurrentFrame => (int)MathF.Floor(_currentFrame);
        private Color _color;
        public Color Color => _color;

        private Vector2 _orientation;
        private Vector2 _moveDirection;
        public Vector2 MoveDirection => _moveDirection;
        protected float _currentRotation;
        public float CurrentRotation => _currentRotation;
        protected Vector2 _currentScale;
        public Vector2 CurrentScale => _currentScale;
        protected float _baseSpeed;
        protected float _speed;
        public float CurrentSpeed => _speed * _baseSpeed;
        protected float _animationSpeed;
        private string _currentAnimation;
        private int _currentAnimationFrameCount;
        private float _currentAnimationSpeed;

        public Vector2 drawOffset;

        public bool CanChangeDirection { get; set; }

        private Action _onAnimationEnd;
        protected Action<int> _onAnimationFrame;

        public Character(string spriteSheetAsset, Game game) : base(game)
        {
            _spriteSheetAsset = spriteSheetAsset;
            _currentScale = Vector2.One;
            _currentFrame = 0;
            _color = Color.White;
            CanChangeDirection = true;
            LookTo(new Vector2(1, 0));
            _animationSpeed = 1f;
            _speed = 1f;
            Visible = true;
        }

        protected override void LoadContent()
        {
            _spriteSheet = new SpriteSheet(Game.Content, _spriteSheetAsset, 8, 8);
        }

        public void SetColor(Color color)
        {
            _color = color;
        }

        public void SetBaseSpeed(float speed)
        {
            _baseSpeed = speed;
        }

        public void SetSpeed(float speed)
        {
            _speed = speed;
        }

        public void SetAnimationSpeed(float animationSpeed)
        {
            _animationSpeed = animationSpeed;
        }

        public void SetFrame(int frameIndex)
        {
            if (frameIndex > 0 && frameIndex < _currentAnimationFrameCount)
            {
                _currentFrame = frameIndex;
            }
        }

        public void SetAnimation(string animationName, Action onAnimationEnd = null)
        {
            if (_currentAnimation != animationName && _spriteSheet.HasAnimation(animationName))
            {
                _currentAnimation = animationName;
                _currentAnimationFrameCount = _spriteSheet.GetAnimationFrameCount(animationName);
                _currentAnimationSpeed = _spriteSheet.GetAnimationSpeed(animationName);

                _currentFrame = 0;

                _onAnimationEnd = onAnimationEnd;
            }
        }

        public void SetScale(Vector2 scale)
        {
            _currentScale = scale;
        }

        public void MoveTo(Vector2 position)
        {
            _position = position;
        }

        public void MoveBy(Vector2 translation)
        {
            _position += translation;
        }

        public void LookTo(Vector2 direction, bool rotate = true)
        {
            _moveDirection = Vector2.Normalize(direction);
            if (rotate)
            {
                if (direction.X != 0)
                {
                    _orientation.X = direction.X;
                    _currentScale.X = direction.X;
                }
                if (direction.Y != 0)
                {
                    _orientation.Y = direction.Y;
                    _currentScale.Y = -_orientation.X * _orientation.Y;
                }
                else
                {
                    _currentScale.Y = 1;
                }
                _orientation.Y = direction.Y;
                _currentRotation = _orientation.X * _orientation.Y * MathF.PI / 2;
            }
        }

        public virtual void Move(float deltaTime)
        {
            _position += _moveDirection * CurrentSpeed * deltaTime;
        }

        public void Animate(float deltaTime)
        {
            int previousFrame = (int)MathF.Floor(_currentFrame);
            _currentFrame = _currentFrame + deltaTime * _currentAnimationSpeed * _animationSpeed;
            if (_currentFrame > _currentAnimationFrameCount)
            {
                _currentFrame = 0;
                if (_onAnimationEnd != null)
                {
                    _onAnimationEnd?.Invoke();
                }
            }
            int newFrame = (int)MathF.Floor(_currentFrame);
            if (previousFrame != newFrame)
            {
                _onAnimationFrame?.Invoke(newFrame);
            }
        }

        public void Die()
        {
            _enabled = false;
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteSheet.DrawAnimationFrame(_currentAnimation, CurrentFrame, SpriteBatch, new Vector2(PixelPositionX, PixelPositionY) + drawOffset, _currentRotation, _currentScale, _color);
        }

        public Color GetPixel(int x, int y)
        {
            int scaledX;
            if (_currentScale.X < 0)
                scaledX = SpriteSheet.FrameWidth - x - 1;
            else
                scaledX = x;
            scaledX = (int)MathF.Floor(scaledX * MathF.Abs(_currentScale.X));

            int scaledY;
            if (_currentScale.Y < 0)
                scaledY = SpriteSheet.FrameHeight - y - 1;
            else
                scaledY = y;
            scaledY = (int)MathF.Floor(scaledY * MathF.Abs(_currentScale.Y));

            return SpriteSheet.GetPixel(_spriteSheet.GetAbsoluteFrameIndex(_currentAnimation, CurrentFrame), scaledX, scaledY);
        }
    }
}
