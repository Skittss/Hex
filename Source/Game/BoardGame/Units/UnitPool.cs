using System;

namespace Hex.BoardGame
{
    public static class UnitPool
    {
        //store all the different unit types that can appear in the game.
        private static Type[] unitPool = new Type[] 
        {
            typeof(Mage),
            typeof(IceMage),
            typeof(Crab),
            typeof(Recruit),
            typeof(Rider)
        };

        public static Type[] GetUnitPool()
        {
            return unitPool;
        }
    }


}
