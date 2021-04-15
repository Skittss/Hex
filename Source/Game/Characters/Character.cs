using Hex.BoardGame;

namespace Hex.Characters
{
    public abstract class Character
    {
        protected int linkId;

        public Character()
        {

        }

        public abstract void PerformAbility(HexGrid grid, Player player);
    }
}
