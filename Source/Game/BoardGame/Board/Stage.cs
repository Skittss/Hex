using System;
using Hex.Pools;
using System.Linq;

namespace Hex.BoardGame
{
    public class Stage
    {
        private int stageNumber;
        public int StageNumber { get { return stageNumber; } }

        private int stageLength;
        public int Length { get { return stageLength; } }
        private int difficulty;
        private RoundTypes[] rounds;
        public RoundTypes[] Rounds { get { return rounds; } }

        //S curve data
        private static int maxDifficulty = 7;
        private static float midDifficultyApprox = 4.3f;
        private static float growth = 0.55f;

        //Stage randomiser information
        private static int vsPlayerGap = 1;
        private static float chanceOfExtraVsPlayer = 0.5f;
        
        public enum RoundTypes
        {
            vsAi,
            vsPlayer
        }

        private int MapStageDifficulty(int stageNumber)
        {
            //convert a stage number to a difficulty number, the difficulty determines which pool to choose stage length from.
            //apply an S curve so that difficulty increases slowly, then lienarly until later stages where it caps out.
            int difficulty = (int)Math.Ceiling(maxDifficulty / (1 + Math.Pow(Math.E, -growth * (stageNumber - midDifficultyApprox))));
            return difficulty;
        }

        public bool IsRoundVsAi(int index)
        {
            return rounds[index] == RoundTypes.vsAi;
        }

        public Unit[] GenerateAiUnits()
        {
            //generate amount of units equal to the difficulty.
            Unit[] units = new Unit[difficulty];
            Type[] unitPool = UnitPool.GetUnitPool();
            Random random = new Random();
            for (int i = 0; i < difficulty; i++)
            {
                //instantiate a new unit of random type from the unit pool.
                //  - first get the constructor of a random unit in the pool.
                var constructors = unitPool[random.Next(0, unitPool.Length)].GetConstructors();

                //instantiate constructor with ownerId 3 (AI opponent)
                units[i] = (Unit)constructors.First().Invoke(new object[] { 3 });
            }
            return units;
        }

        public Stage(int stageNumber)
        {
            //Code for creating a stage structure.
            this.stageNumber = stageNumber;

            //generate a difficulty
            difficulty = MapStageDifficulty(stageNumber);
            stageLength = ProbabilityPools.stageLengthPools[difficulty].GetRandom();
            rounds = new RoundTypes[stageLength];

            //generate semi-random stage structure - player rounds should be at least 1 round apart.
            //note random.next is EXCLUSIVE so stageLength - 1 would miss a result
            int position = -vsPlayerGap;
            float probability = 1;
            while ((stageLength - 1) - position > vsPlayerGap)   //while there is space for another vsPlayer round
            {
                if (new Random().NextDouble() <= probability)
                {
                    //reduce the probability of *another* vs player round occuring to a half of its value.
                    probability *= chanceOfExtraVsPlayer;
                    position = new Random().Next(position + vsPlayerGap + 1, stageLength);
                    rounds[position] = RoundTypes.vsPlayer;
                }
                else
                {
                    break;
                    //break from the loop - no extra vsPlayer is added.
                }
            }

            rounds = rounds.Select(round => round == null ? RoundTypes.vsAi : round).ToArray();
        }
    }
}
