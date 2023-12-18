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
        private List<Point> _waypoints = new List<Point>();
        private int _currentWaypoint;
        private int _currentTargetWaypoint;

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

            if (_waypoints.Count > 0)
            {
                Move((float)gameTime.ElapsedGameTime.TotalSeconds);
                if (PixelPositionX == _waypoints[_currentTargetWaypoint].X && PixelPositionY == _waypoints[_currentTargetWaypoint].Y) 
                {
                    SetCurrentWaypoint(GetNextWaypoint(_currentWaypoint));
                }
            }
        }

        public void AddWaypoint(int x, int y)
        {
            _waypoints.Add(new Point(x,y));
        }

        private int GetNextWaypoint(int waypointIndex)
        {
            return (waypointIndex + 1) % _waypoints.Count;
        }

        private void SetCurrentWaypoint(int index)
        {
            _currentWaypoint = index;
            _currentTargetWaypoint = GetNextWaypoint(index);
            LookTo((_waypoints[_currentTargetWaypoint] - _waypoints[_currentWaypoint]).ToVector2(), rotate:false);
        }

        public void Reset()
        {
            if (_waypoints.Count > 0)
            {
                MoveTo(new Vector2(_waypoints[0].X, _waypoints[0].Y));
                SetCurrentWaypoint(0);
            }
        }
    }
}
