using System.Collections.Generic;
using System.Linq;

namespace Hex.BoardGame
{
    /// <summary>
    /// class for generating ids for individual units used in the game.
    /// Though not entirely necessary, makes handling unit ids easier to debug (lower ID values)
    /// </summary>
    public class UnitIdLeaser
    {
        //maintain a list of ids of existing units.
        private List<int> leasedIds = new List<int>();

        private static UnitIdLeaser instance;
        public static UnitIdLeaser Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UnitIdLeaser();
                }
                return instance;
            }
        }

        public UnitIdLeaser()
        {
            
        }

        public int GenerateNewId()
        {
            if (leasedIds.Count == 0)
            {
                leasedIds.Add(0);
                return 0;
            }
            else
            {
                int id = leasedIds.Max() + 1;
                leasedIds.Add(id);
                return id;
            }
        }
        public void ReturnId(int id)
        {
            leasedIds.Remove(id);
        }

        public void Reset()
        {
            leasedIds = new List<int>();
        }
    }
}

