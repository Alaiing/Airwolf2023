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

        public void AddSwitch(Point position, int switchSection, int gateSection)
        {
            _levelSections[switchSection].AddSwitch(position, gateSection);
        }

        public int GetSwitchGateApprox(Point position, int switchSection)
        {
            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                {
                    int gateSection = GetSwitchGate(position + new Point(x, y), switchSection);
                    if (gateSection >=0)
                    {
                        return gateSection;
                    }
                }

            return -1;
        }

        public int GetSwitchGate(Point position, int switchSection)
        {
            return _levelSections[switchSection].GetSwitchGate(position);
        }

        public void Reset()
        {
            foreach (LevelSection section in _levelSections)
            {
                section.Reset();
                section.gateOpen = false;
            }
        }

        public void ResetGates()
        {
            foreach (LevelSection section in _levelSections)
            {
                section.gateOpen = false;
            }
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

            for (int i = 0; i < backgroundSheet.FrameCount; i++)
            {
                level.AddSection(new LevelSection(backgroundSheet, i, game), i);
            }

            Enemy newEnemy = new Enemy("parabola", 8, 16, 10, game);
            newEnemy.SetScale(new Vector2(1, -1));
            level.GetSection(0).AddEnemy(newEnemy, new Vector2(40, 32));
            level.GetSection(0).AddEnemy(new Enemy("parabola", 8, 16, 10, game), new Vector2(134, 79));
            level.AddSwitch(new Point(44, 80), 0, 2);

            newEnemy = new Enemy("parabola", 8, 16, 10, game);
            newEnemy.SetScale(new Vector2(1, -1));
            level.GetSection(4).AddEnemy(newEnemy, new Vector2(32, 32));
            level.GetSection(4).AddEnemy(new Enemy("parabola", 8, 16, 10, game), new Vector2(126, 64));

            newEnemy = new Enemy("pinata", 8, 16, 20, game);
            newEnemy.AddWaypoint(42, 48, 0f);
            newEnemy.AddWaypoint(101, 48, 0f);
            newEnemy.AddWaypoint(101, 67, 0f);
            newEnemy.AddWaypoint(42, 67, 0f);
            newEnemy.SetBaseSpeed(20f);
            level.GetSection(8).AddEnemy(newEnemy, new Vector2(42, 48));
            newEnemy.Reset();
            newEnemy = new Enemy("parabola", 8, 16, 10, game);
            newEnemy.SetScale(new Vector2(1, -1));
            level.GetSection(8).AddEnemy(newEnemy, new Vector2(88, 32));
            level.GetSection(8).AddEnemy(new Enemy("parabola", 8, 16, 10, game), new Vector2(16, 63));

            newEnemy = new Enemy("parabola", 8, 16, 10, game);
            newEnemy.SetScale(new Vector2(1, -1));
            level.GetSection(9).AddEnemy(newEnemy, new Vector2(48, 16));

            newEnemy = new Enemy("parabola", 8, 16, 10, game);
            newEnemy.SetScale(new Vector2(1, -1));
            level.GetSection(9).AddEnemy(newEnemy, new Vector2(119, 16));

            newEnemy = new Enemy("monolith", 8, 16, 50, game);
            newEnemy.AddWaypoint(83, 39, 2f);
            newEnemy.AddWaypoint(28, 39, 0f);
            newEnemy.AddWaypoint(83, 39, 2f);
            newEnemy.AddWaypoint(138, 39, 0f);
            newEnemy.SetBaseSpeed(100f);
            level.GetSection(9).AddEnemy(newEnemy, new Vector2(83, 39));
            newEnemy.Reset();
            newEnemy = new Gate("lightning_gate", 47, 12, game);
            level.GetSection(9).AddEnemy(newEnemy, new Vector2(80, 65));

            newEnemy = new Enemy("parabola", 8, 16, 10, game);
            level.GetSection(10).AddEnemy(newEnemy, new Vector2(112, 63));
            newEnemy = new Enemy("monolith", 8, 16, 50, game);
            newEnemy.AddWaypoint(24, 69, 0f);
            newEnemy.AddWaypoint(24, 24, 0f);
            newEnemy.SetBaseSpeed(16f);
            level.GetSection(10).AddEnemy(newEnemy, new Vector2(24, 69));
            newEnemy.Reset();
            newEnemy = new Enemy("monolith", 8, 16, 50, game);
            newEnemy.AddWaypoint(126, 31, 0f);
            newEnemy.AddWaypoint(107, 51, 0f);
            newEnemy.AddWaypoint(87, 31, 0f);
            newEnemy.AddWaypoint(107, 12, 0f);
            newEnemy.SetBaseSpeed(40f);
            level.GetSection(10).AddEnemy(newEnemy, new Vector2(24, 69));
            newEnemy.Reset();
            newEnemy = new Gate("metal_gate", 8, 47, game);
            level.GetSection(10).AddGate((Gate)newEnemy, new Vector2(152, 16));
            level.AddSwitch(new Point(36, 80), 10, 10);


            newEnemy = new Enemy("thrusters", 8, 16, -1, game);
            newEnemy.SetAnimationSpeed(4f);
            level.GetSection(5).AddEnemy(newEnemy, new Vector2(64, 47));
            newEnemy = new Enemy("thrusters", 8, 16, -1, game);
            newEnemy.SetAnimationSpeed(2f);
            level.GetSection(5).AddEnemy(newEnemy, new Vector2(88, 47));
            newEnemy = new Enemy("thrusters", 8, 16, -1, game);
            newEnemy.SetAnimationSpeed(1f);
            level.GetSection(5).AddEnemy(newEnemy, new Vector2(112, 47));
            newEnemy = new Enemy("pinata", 8, 16, 20, game);
            newEnemy.AddWaypoint(144, 63, 20f);
            newEnemy.AddWaypoint(54, 63, 0f, teleport:true);
            newEnemy.SetBaseSpeed(40f);
            level.GetSection(5).AddEnemy(newEnemy, new Vector2(144, 63));


            newEnemy = new Enemy("parabola", 8, 16, 10, game);
            newEnemy.SetScale(new Vector2(1, -1));
            level.GetSection(6).AddEnemy(newEnemy, new Vector2(24, 16));
            newEnemy = new Enemy("pinata", 8, 16, 20, game);
            newEnemy.AddWaypoint(44, 31);
            newEnemy.AddWaypoint(102, 31);
            newEnemy.SetBaseSpeed(35f);
            level.GetSection(6).AddEnemy(newEnemy, new Vector2(80, 65));
            newEnemy.Reset();
            newEnemy = new Gate("lightning_gate", 47, 12, game);
            level.GetSection(6).AddGate((Gate)newEnemy, new Vector2(80, 81));

            newEnemy = new Enemy("parabola", 8, 16, 10, game);
            newEnemy.SetScale(new Vector2(1, -1));
            level.GetSection(2).AddEnemy(newEnemy, new Vector2(28, 16));
            newEnemy = new Enemy("parabola", 8, 16, 10, game);
            newEnemy.SetScale(new Vector2(1, -1));
            level.GetSection(2).AddEnemy(newEnemy, new Vector2(60, 16));
            newEnemy = new Enemy("disco_ball", 8, 16, 50, game);
            newEnemy.AddWaypoint(114, 36);
            newEnemy.AddWaypoint(21, 36);
            newEnemy.SetBaseSpeed(30f);
            level.GetSection(2).AddEnemy(newEnemy, new Vector2(80, 65));
            newEnemy.Reset();
            newEnemy = new Gate("metal_gate", 8, 63, game);
            level.GetSection(2).AddGate((Gate)newEnemy, new Vector2(152, 16));
            level.AddSwitch(new Point(36, 80), 2, 6);

            newEnemy = new Enemy("monolith", 8, 16, 50, game);
            newEnemy.AddWaypoint(93, 47);
            newEnemy.AddWaypoint(40, 47);
            newEnemy.AddWaypoint(93, 47, 3f);
            newEnemy.SetBaseSpeed(28f);
            level.GetSection(11).AddEnemy(newEnemy, new Vector2(93, 47));
            newEnemy.Reset();
            newEnemy = new Enemy("monolith", 8, 16, 50, game);
            newEnemy.AddWaypoint(81, 16);
            newEnemy.AddWaypoint(81, 55);
            newEnemy.SetBaseSpeed(20f);
            level.GetSection(11).AddEnemy(newEnemy, new Vector2(81, 16));
            newEnemy.Reset();
            level.AddSwitch(new Point(100, 80), 11, 3);

            newEnemy = new Enemy("monolith", 8, 16, 50, game);
            newEnemy.SetAnimationSpeed(4);
            level.GetSection(3).AddEnemy(newEnemy, new Vector2(56, 32));
            newEnemy = new Enemy("monolith", 8, 16, 50, game);
            newEnemy.SetAnimationSpeed(2);
            newEnemy.AddWaypoint(121, 32);
            newEnemy.AddWaypoint(40, 32);
            newEnemy.SetBaseSpeed(20f);
            level.GetSection(3).AddEnemy(newEnemy, new Vector2(81, 16));
            newEnemy.Reset();
            newEnemy = new Enemy("monolith", 8, 16, 50, game);
            newEnemy.SetAnimationSpeed(2);
            newEnemy.AddWaypoint(105, 48);
            newEnemy.AddWaypoint(24, 48);
            newEnemy.SetBaseSpeed(30f);
            level.GetSection(3).AddEnemy(newEnemy, new Vector2(81, 16));
            newEnemy.Reset();
            newEnemy = new Gate("lightning_gate", 47, 12, game);
            newEnemy.SetScale(new Vector2(40f / 47f, 1));
            level.GetSection(3).AddGate((Gate)newEnemy, new Vector2(32, 65));

            newEnemy = new Enemy("pinata", 8, 16, 20, game);
            newEnemy.AddWaypoint(52, 48);
            newEnemy.AddWaypoint(26, 48);
            newEnemy.SetBaseSpeed(15f);
            level.GetSection(7).AddEnemy(newEnemy, new Vector2(144, 63));
            newEnemy = new Enemy("pinata", 8, 16, 20, game);
            newEnemy.AddWaypoint(119, 61);
            newEnemy.AddWaypoint(81, 61);
            newEnemy.AddWaypoint(81, 30);
            newEnemy.AddWaypoint(119, 30);
            newEnemy.SetBaseSpeed(40f);
            level.GetSection(7).AddEnemy(newEnemy, new Vector2(144, 63));
            newEnemy.Reset();
            newEnemy = new Enemy("disco_ball", 8, 16, 50, game);
            newEnemy.AddWaypoint(88, 30);
            newEnemy.AddWaypoint(88, 63);
            newEnemy.SetBaseSpeed(20f);
            level.GetSection(7).AddEnemy(newEnemy, new Vector2(80, 65));
            newEnemy.Reset();
            newEnemy = new Gate("prisoner_gate", 24, 36, game);
            level.GetSection(7).AddGate((Gate)newEnemy, new Vector2(128, 20), gateCount:2);
            level.AddSwitch(new Point(28, 96), 7, 7);
            level.AddSwitch(new Point(116, 80), 7, 7);
            newEnemy = new Prisoner("prisoner", 7, 15, -1, game);
            level.GetSection(7).AddEnemy(newEnemy, new Vector2(136, 31));


            return level;
        }
    }
}
