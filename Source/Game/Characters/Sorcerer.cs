using Hex.BoardGame;
using System.Linq;

namespace Hex.Characters
{
    public class Sorcerer : Character
    {
        public const string SpriteReference = "CharacterSelect/Buttons/SorcererButton";
        public const string AbilityDescription =
            "Increase the range of all ally units on board by two tiles.";
        
        public Sorcerer()
        {

        }

        public override void PerformAbility(HexGrid grid, Player player)
        {
            //increase the range of all owned units on the board by two tiles.
            foreach (Unit unit in grid.Units.Where(unit => unit.OwnerId == player.Id))
            {
                //increase the range by two.
                unit.Stats.Range.Augment(x => x + 2);
                unit.SetAugmentFlag();
            }
        }
    }
}
