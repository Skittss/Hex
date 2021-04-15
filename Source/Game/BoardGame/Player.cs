using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Engine.GridUtils;
using Hex.Characters;

namespace Hex.BoardGame
{
    public class Player
    {
        private int id;
        public int Id { get { return id; } }

        private Color colour;
        public Color Colour { get { return colour; } }

        private Character character;
        private Bench bench = new Bench();

        private bool abilityUsed = false;
        public bool AbilityUsed { get { return abilityUsed; } }

        private static int baseCap = 3;
        private int unitCap = baseCap;
        public int UnitCap { get { return unitCap; } }
        //cost to increase the unit cap increases by one each time increased.
        public int LevelUnitCapCost { get { return unitCap + 1 - baseCap; } }

        private int money = 3;
        public int Money
        {
            get { return money; }
            set { money = value; }
        }

        public readonly UnitShop Shop;

        private int winProgress = 0;
        public int WinProgress { get { return winProgress; } }
        public readonly int WinThreshold;

        private Texture2D characterSprite;
        public Texture2D CharacterSprite
        {
            get { return characterSprite; }
            set { characterSprite = value; }
        }

        //bounding coordinates of grid half owned by player
        private Vector2 gridHalfStart;
        private Vector2 gridHalfEnd;

        public Player(int id, Type characterType, GraphicsDevice graphics, Texture2D characterSprite, Color colour, int winThreshold)
        {
            this.id = id;
            this.colour = colour;

            var constructors = characterType.GetConstructors();
            character = (Character)constructors.First().Invoke(new object[] { });

            this.characterSprite = characterSprite;
            Shop = new UnitShop(graphics, this);    
            WinThreshold = winThreshold;

            //add starting units to bench; player starts with 3 crabs
            for (int x = 0; x < 3; x++)
            {
                new Crab(id).AddToBench(this);
            }
        }

        public void AssignHalf(Vector2 start, Vector2 end)
        {
            gridHalfStart = start;
            gridHalfEnd = end;
        }

        public Vector3 GetRandomPositionInHalf()
        {
            Random rand = new Random();
            Vector2 pos = new Vector2(rand.Next((int)gridHalfStart.X, (int)gridHalfEnd.X + 1), rand.Next((int)gridHalfStart.Y, (int)gridHalfEnd.Y + 1));
            return CoordConverter.OffsetToCube(pos);
        }

        public bool IsPointInHalf(Vector2 point, bool benchFlag)
        {
            if (benchFlag)
            {
                return true;
            }
            return point.X >= gridHalfStart.X && point.X <= gridHalfEnd.X
                && point.Y >= gridHalfStart.Y && point.Y <= gridHalfEnd.Y;
        }

        public void IncreaseUnitCap()
        {
            //cost increases linearly by one money for each level
            if (money >= LevelUnitCapCost)
            {
                money -= LevelUnitCapCost;
                unitCap += 1;
            }
        }

        public void PerformAbility(HexGrid grid)
        {
            if (!abilityUsed)
            {
                character.PerformAbility(grid, this);
                abilityUsed = true;
            }
        }

        public void IncrementWinProgress()
        {
            winProgress += 1;
        }

        //methods for managing each player's bench.
        public bool IsBenchFull()
        {
            return bench.IsFull();
        }

        public Unit[] GetBenchUnits()
        {
            return bench.GetBenchUnits();
        }

        public int GetBenchCount()
        {
            return bench.Count;
        }

        public int AddUnitToBench(Unit unit)
        {
            //places the unit at the next available position
            return bench.AddToBench(unit);
        }

        public void AddUnitToBench(Unit unit, int position)
        {
            bench.AddToBench(unit, position);
        }

        public void RemoveUnitFromBench(Unit unit)
        {
            bench.RemoveFromBench(unit);
        }

        public void SwapTwoBenchUnits(Unit unitA, Unit unitB)
        {
            bench.SwapTwoUnits(unitA, unitB);
        }
    }
}
