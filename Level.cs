using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Airwolf2023
{
    public class Level
    {
        private readonly LevelSection[] _levelSections = new LevelSection[12];
        private int _currentSectionIndex;
        public int CurrentSectionIndex => _currentSectionIndex;
        public int CurrentBackgroundFrame => _levelSections[_currentSectionIndex].Frame;

        public List<Enemy> CurrentEnemies => _currentSectionIndex >= 0 ? _levelSections[_currentSectionIndex].Enemies : null;

        public Level()
        {
            _currentSectionIndex = -1;
        }

        public void AddSection(LevelSection section, int index)
        {
            _levelSections[index] = section;
        }

        public void SetCurrentSection(int sectionIndex)
        {
            if (_currentSectionIndex >= 0)
            {
                _levelSections[_currentSectionIndex].Deactivate();
            }
            _currentSectionIndex = sectionIndex;
            _levelSections[_currentSectionIndex].Activate();
            _levelSections[_currentSectionIndex].UnPause();
        }

        public LevelSection GetSection(int sectionIndex)
        {
            return _levelSections[sectionIndex];
        }

        public void Activate(Game game)
        {
            foreach (var section in _levelSections)
            {
                if (section != null)
                {
                    game.Components.Add(section);
                    section.Deactivate();
                }
            }
        }

        public void Deactivate(Game game)
        {
            if (_currentSectionIndex >= 0)
            {
                _levelSections[_currentSectionIndex].Deactivate();
            }
            foreach (var section in _levelSections)
            {
                if (section != null)
                {
                    game.Components.Remove(section);
                }
            }
        }

        public void Pause()
        {
            _levelSections[_currentSectionIndex].Pause();
        }

        public static Level CreateLevel(SpriteSheet backgroundSheet, Game game)
        {
            Level level = new Level();

            for (int i = 0; i< backgroundSheet.FrameCount; i++)
            {
                level.AddSection(new LevelSection(backgroundSheet, i, game), i); 
            }

            Enemy newEnemy = new Enemy("parabola", game);
            newEnemy.SetScale(new Vector2(1, -1));
            level.GetSection(0).AddEnemy(newEnemy, new Vector2(40, 32));
            level.GetSection(0).AddEnemy(new Enemy("parabola", game), new Vector2(134, 79));

            newEnemy = new Enemy("parabola", game);
            newEnemy.SetScale(new Vector2(1, -1));
            level.GetSection(4).AddEnemy(newEnemy, new Vector2(32, 32));
            level.GetSection(4).AddEnemy(new Enemy("parabola", game), new Vector2(126, 64));

            newEnemy = new Enemy("pinata", game);
            newEnemy.AddWaypoint(42, 48);
            newEnemy.AddWaypoint(101, 48);
            newEnemy.AddWaypoint(101, 67);
            newEnemy.AddWaypoint(42, 67);
            newEnemy.SetBaseSpeed(20f);
            level.GetSection(8).AddEnemy(newEnemy, new Vector2(42, 48));
            newEnemy.Reset();
            newEnemy = new Enemy("parabola", game);
            newEnemy.SetScale(new Vector2(1, -1));
            level.GetSection(8).AddEnemy(newEnemy, new Vector2(88, 32));
            level.GetSection(8).AddEnemy(new Enemy("parabola", game), new Vector2(16, 63));


            return level;
        }
    }
}
