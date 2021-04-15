using System.Linq;

namespace Hex.BoardGame
{
    public class Bench
    {
        private static int benchCapacity = 7;
        private Unit[] bench = new Unit[benchCapacity];
        public int Count { get { return bench.Where(unit => unit != null).Count(); } }

        public Bench()
        {

        }

        public bool IsFull()
        {
            return Count == benchCapacity;
        }

        public int AddToBench(Unit unit)
        {
            //gets the next available position and puts the unit there.
            if (!IsFull())
            {
                for (int i = 0; i < benchCapacity; i++)
                {
                    if (bench[i] == null)
                    {
                        bench[i] = unit;
                        unit.BenchPosition = i;
                        unit.OnBench = true;
                        return i;

                    }
                }
            }
            //return -1 if unable to add unit to bench.
            // (this is a fail case, avoid adding with full bench).
            return -1;
        }

        public void AddToBench(Unit unit, int position)
        {
            if (bench[position] == null) {
                bench[position] = unit;
                unit.BenchPosition = position;
                unit.OnBench = true;
            }
        }

        public void RemoveFromBench(Unit unit)
        {
            int index = GetUnitPositionInBench(unit);
            bench[index] = null;
        }

        public Unit[] GetBenchUnits()
        {
            return bench;
        }

        private int GetUnitPositionInBench(Unit unit)
        {
            for (int count = 0; count < bench.Length; count++)
            {
                if (bench[count] != null)
                {
                    if (bench[count].id == unit.id)
                    {
                        return count;
                    }
                }
            }
            //not found - return -1 as flag.
            return -1;
        }

        public void SwapTwoUnits(Unit unitA, Unit unitB)
        {
            int posA = GetUnitPositionInBench(unitA);
            int posB = GetUnitPositionInBench(unitB);

            unitA.BenchPosition = posB;
            unitB.BenchPosition = posA;

            bench[posA] = unitB;
            bench[posB] = unitA;
        }
    }
}