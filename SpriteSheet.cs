using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Oudidon
{
    public class SpriteSheet
    {
        public struct Animation
        {
            public int startingFrame;
            public int endFrame;
            public float speed;
            public readonly int FrameCount => Math.Abs(endFrame - startingFrame) + 1;
            public readonly int AnimationDirection => Math.Sign(endFrame - startingFrame);
        }

        private readonly Texture2D _texture;
        private Color[] _originalTextureColors;
        private Color[] _currentTextureColors;
        public Texture2D Texture => _texture;
        private Rectangle[] allFrames;
        private Dictionary<string, Animation> _animations = new Dictionary<string, Animation>();

        public int FrameCount => allFrames.Length;

        private Vector2 _spritePivot;
        public Vector2 SpritePivot => _spritePivot;

        public int FrameWidth;
        public int FrameHeight;

        public int LeftMargin;
        public int RightMargin;
        public int TopMargin;
        public int BottomMargin;

        public SpriteSheet(ContentManager content, string asset, int frameWidth, int frameHeight, int spritePivotX = 0, int spritePivotY = 0)
        {
            _texture = content.Load<Texture2D>(asset);
            _currentTextureColors = new Color[_texture.Width * _texture.Height];
            _originalTextureColors = new Color[_texture.Width * _texture.Height];
            _texture.GetData(_currentTextureColors);
            _texture.GetData(_originalTextureColors);
            _spritePivot = new Vector2(spritePivotX, spritePivotY);
            LeftMargin = spritePivotX;
            RightMargin = frameWidth - spritePivotX;
            TopMargin = spritePivotY;
            BottomMargin = frameHeight - spritePivotY;
            FrameWidth = frameWidth;
            FrameHeight = frameHeight;
            InitFrames(frameWidth, frameHeight);
        }

        private void InitFrames(int spriteWidth, int spriteHeight)
        {
            int xCount = _texture.Width / spriteWidth;
            int yCount = _texture.Height / spriteHeight;
            allFrames = new Rectangle[xCount * yCount];

            for (int x = 0; x < xCount; x++)
            {
                for (int y = 0; y < yCount; y++)
                {
                    allFrames[x + y * xCount] = new Rectangle(x * spriteWidth, y * spriteHeight, spriteWidth, spriteHeight);
                }
            }
        }

        public void RegisterAnimation(string name, int startingFrame, int endingFrame, float animationSpeed)
        {
            _animations.Add(name, new Animation { startingFrame = startingFrame, endFrame = endingFrame, speed = animationSpeed });
        }

        public bool HasAnimation(string name)
        {
            return !string.IsNullOrEmpty(name) && _animations.ContainsKey(name);
        }

        public int GetAnimationFrameCount(string animationName)
        {
            if (_animations.TryGetValue(animationName, out Animation animation))
            {
                return animation.FrameCount;
            }

            return -1;
        }

        public int GetAnimationDirection(string animationName)
        {
            if (_animations.TryGetValue(animationName, out Animation animation))
            {
                return animation.AnimationDirection;
            }

            return 1;
        }

        public float GetAnimationSpeed(string animationName)
        {
            if (_animations.TryGetValue(animationName, out Animation animation))
            {
                return animation.speed;
            }

            return 0f;
        }

        private int GetTextureIndex(int frameIndex, int x, int y)
        {
            Rectangle frameRectangle = allFrames[frameIndex];
            return (x + frameRectangle.X) + (y + frameRectangle.Y) * _texture.Width;
        }

        public Color GetPixel(int frameIndex, int x, int y)
        {
            if (x >= 0 && x < FrameWidth && y >= 0 && y < FrameHeight)
            {
                Rectangle frameRectangle = allFrames[frameIndex];
                int textureIndex = GetTextureIndex(frameIndex, x, y);
                if (textureIndex >= 0 && textureIndex < _currentTextureColors.Length)
                {
                    return _currentTextureColors[(x + frameRectangle.X) + (y + frameRectangle.Y) * _texture.Width];
                }
            }

            return new Color(0,0,0,0);
        }

        public void SetPixel(int frameIndex, int x, int y, Color newColor)
        {
            _currentTextureColors[GetTextureIndex(frameIndex, x, y)] = newColor;
            _texture.SetData(_currentTextureColors);
        }

        public void RestoreOriginalTexture()
        {
            _texture.SetData(_originalTextureColors);
            _texture.GetData(_currentTextureColors);
        }

        public void DrawAnimationFrame(string animationName, int frameIndex, SpriteBatch spriteBatch, Vector2 position, float rotation, Vector2 scale, Color color)
        {
            int frame = GetAbsoluteFrameIndex(animationName, frameIndex);

            if (frame >= 0)
            {
                DrawFrame(frame, spriteBatch, position, rotation, scale, color);
            }
        }

        public int GetAbsoluteFrameIndex(string animationName, int frameIndex)
        {
            if (_animations.TryGetValue(animationName, out Animation animation))
            {
                return animation.startingFrame + frameIndex * animation.AnimationDirection;
            }

            return -1;
        }

        public void DrawFrame(int frameIndex, SpriteBatch spriteBatch, Vector2 position, float rotation, Vector2 scale, Color color)
        {
            if (frameIndex < allFrames.Length)
            {
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (scale.X < 0)
                {
                    spriteEffects |= SpriteEffects.FlipHorizontally;
                    scale.X = -scale.X;
                }
                if (scale.Y < 0)
                {
                    spriteEffects |= SpriteEffects.FlipVertically;
                    scale.Y = -scale.Y;
                }
                spriteBatch.Draw(_texture, position, allFrames[frameIndex], color, rotation, _spritePivot, scale, spriteEffects, 0);
            }
        }
    }
}