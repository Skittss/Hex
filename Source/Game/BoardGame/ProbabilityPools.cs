using Engine.Pools;
using System.Collections.Generic;

namespace Hex.Pools
{
    public static class ProbabilityPools
    {
        public static Dictionary<int, WeightedPool<int>> stageLengthPools;

        public static void AssignPools()
        {
            //Stage lengths against stage number. (i.e. chances of a generated stage being
            //  a certain length for a certain difficulty are specified below.)

            //Somewhat verbose, but allows for complete control of probabilities here for
            // more consistent stage generation.
            WeightedPool<int> poolOne = new WeightedPool<int>();
            poolOne.Add(2, 60);
            poolOne.Add(3, 40);

            WeightedPool<int> poolTwo = new WeightedPool<int>();
            poolTwo.Add(3, 20);
            poolTwo.Add(4, 60);
            poolTwo.Add(5, 20);

            WeightedPool<int> poolThree = new WeightedPool<int>();
            poolThree.Add(4, 40);
            poolThree.Add(5, 40);
            poolThree.Add(6, 20);

            WeightedPool<int> poolFour = new WeightedPool<int>();
            poolFour.Add(4, 30);
            poolFour.Add(5, 50);
            poolFour.Add(6, 20);

            WeightedPool<int> poolFive = new WeightedPool<int>();
            poolFive.Add(4, 10);
            poolFive.Add(5, 30);
            poolFive.Add(6, 40);
            poolFive.Add(7, 20);

            WeightedPool<int> poolSix = new WeightedPool<int>();
            poolSix.Add(5, 30);
            poolSix.Add(6, 40);
            poolSix.Add(7, 30);

            WeightedPool<int> poolSeven = new WeightedPool<int>();
            poolSeven.Add(6, 20);
            poolSeven.Add(7, 30);
            poolSeven.Add(8, 50);

            stageLengthPools = new Dictionary<int, WeightedPool<int>>()
            {
                {1, poolOne},
                {2, poolTwo},
                {3, poolThree},
                {4, poolFour},
                {5, poolFive},
                {6, poolSix},
                {7, poolSeven},
            };
        }
    }
}
