using Hex.BoardGame;
using System.Linq;

namespace Hex.Characters
{
    public class Knight : Character
    {
        public const string SpriteReference = "CharacterSelect/Buttons/KnightButton";
        public const string AbilityDescription =
            "Increase the damage of all ally units on board by 25%";

        public Knight()
        {

        }

        public override void PerformAbility(HexGrid grid, Player player)
        {
            //increase the damage of all owned units on the board by 25%.
            foreach (Unit unit in grid.Units.Where(unit => unit.OwnerId == player.Id))
            {
                //increase the damage by 1.25x
                unit.Stats.BaseDamage.Augment(x => (int)(x * 1.25));
                unit.SetAugmentFlag();
            }
        }
    }
}
