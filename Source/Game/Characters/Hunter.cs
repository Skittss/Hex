using Hex.BoardGame;
using System.Linq;

namespace Hex.Characters
{
    public class Hunter : Character
    {
        public const string SpriteReference = "CharacterSelect/Buttons/HunterButton";
        public const string AbilityDescription =
            "Increase the maximum move distance of all ally units on board by 2 tiles.";

        public Hunter()
        {

        }

        public override void PerformAbility(HexGrid grid, Player player)
        {
            foreach (Unit unit in grid.Units.Where(unit => unit.OwnerId == player.Id))
            {
                unit.Stats.MoveDistance.Augment(x => x + 2);
                unit.SetAugmentFlag();
            }
        }
    }
}