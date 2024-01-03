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
        struct Waypoint
        {
            public Point position;
            public float duration;
            public bool teleportToNext;
        }
        private List<Waypoint> _waypoints = new List<Waypoint>();
        private int _currentWaypoint;
        private int _currentTargetWaypoint;
        private float _waypointTimer;

        private int _maxLife;
        private int _life;
        public int Life => _life;
        public bool IsDead => _maxLife > 0 == _life <= 0;

        public Enemy(string spriteSheetAsset, int frameWidth, int frameHeight, int maxLife, Game game) : base(spriteSheetAsset, frameWidth, frameHeight, game)
        {
            DrawOrder = 10;
            _maxLife = maxLife;
        }

        protected override void LoadContent()
        {
            _spriteSheet = new SpriteSheet(Game.Content, _spriteSheetAsset, _frameWidth, _frameHeight);
            _spriteSheet.RegisterAnimation("Idle", 0, 3, 8f);
            SetAnimation("Idle");
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Animate(deltaTime);

            if (_waypoints.Count > 0)
            {
                _waypointTimer += deltaTime;
                if (_waypointTimer >= _waypoints[_currentWaypoint].duration)
                {
                    Move((float)gameTime.ElapsedGameTime.TotalSeconds);
                    Vector2 toDestination = _waypoints[_currentTargetWaypoint].position.ToVector2() - Position;
                    Vector2 movementDirection = MoveDirection;

                    if (Vector2.Dot(toDestination, movementDirection) <= 0)
                    {
                        int nextWaypoint = _currentTargetWaypoint;
                        if (_waypoints[_currentTargetWaypoint].teleportToNext)
                        {
                            nextWaypoint = GetNextWaypoint(_currentTargetWaypoint);
                            MoveTo(_waypoints[nextWaypoint].position.ToVector2());
                        }
                        else
                        {
                            MoveTo(_waypoints[_currentTargetWaypoint].position.ToVector2());
                        }
                        SetCurrentWaypoint(nextWaypoint);
                    }
                }
            }
        }

        public void AddWaypoint(int x, int y, float duration = 0f, bool teleport = false)
        {
            _waypoints.Add(new Waypoint { position = new Point(x, y), duration = duration, teleportToNext = teleport });
        }

        private int GetNextWaypoint(int waypointIndex)
        {
            return (waypointIndex + 1) % _waypoints.Count;
        }

        private void SetCurrentWaypoint(int index)
        {
            _currentWaypoint = index;
            _currentTargetWaypoint = GetNextWaypoint(index);
            _waypointTimer = 0;
            if (_waypoints[_currentTargetWaypoint].position != _waypoints[_currentWaypoint].position)
            {
                LookTo((_waypoints[_currentTargetWaypoint].position - _waypoints[_currentWaypoint].position).ToVector2(), rotate: false);
            }
        }

        public void Reset()
        {
            if (_waypoints.Count > 0)
            {
                MoveTo(new Vector2(_waypoints[0].position.X, _waypoints[0].position.Y));
                SetCurrentWaypoint(0);
            }
            _life = _maxLife;
            Visible = true;
        }

        public void Damage(int damage)
        {
            if (_maxLife > 0)
            {
                _life -= damage;
                if (_life <= 0)
                {
                    Visible = false;
                }
            }
        }
    }
}
