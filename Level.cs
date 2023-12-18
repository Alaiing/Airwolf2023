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
        private readonly List<LevelSection> _levelSections = new List<LevelSection>();
        private int _currentSectionIndex;

        public List<Enemy> CurrentEnemies => _currentSectionIndex >= 0 ? _levelSections[_currentSectionIndex].Enemies : null;

        public Level()
        {
            _currentSectionIndex = -1;
        }

        public void AddSection(LevelSection section)
        {
            _levelSections.Add(section);
        }

        public void SetCurrentSection(int sectionIndex)
        {
            if (_currentSectionIndex >= 0)
            {
                _levelSections[_currentSectionIndex].Deactivate();
            }
            _currentSectionIndex = sectionIndex;
            _levelSections[_currentSectionIndex].Activate();
        }


        public void Activate(Game game)
        {
            foreach (var section in _levelSections)
            {
                game.Components.Add(section);
                section.Deactivate();
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
                game.Components.Remove(section);
            }
        }

        public static Level CreateLevel(SpriteSheet backgroundSheet, Game game)
        {
            Level level = new Level();

            LevelSection newSection = new LevelSection(backgroundSheet, 0, game);
            Enemy newEnemy = new Enemy("parabola", game);
            newEnemy.SetScale(new Vector2(1, -1));
            newSection.AddEnemy(newEnemy, new Vector2(40, 32 + Airwolf.BACKGROUND_POSITION_Y));
            newSection.AddEnemy(new Enemy("parabola", game), new Vector2(134, 79 + Airwolf.BACKGROUND_POSITION_Y));
            level.AddSection(newSection);

            newSection = new LevelSection(backgroundSheet, 4, game);
            newSection.AddEnemy(new Enemy("parabola", game), new Vector2(40, 32 + Airwolf.BACKGROUND_POSITION_Y));
            newSection.AddEnemy(new Enemy("parabola", game), new Vector2(134, 80 + Airwolf.BACKGROUND_POSITION_Y));
            level.AddSection(newSection);

            return level;
        }
    }
}
