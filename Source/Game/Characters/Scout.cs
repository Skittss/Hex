using Hex.BoardGame;
using System.Linq;

namespace Hex.Characters
{
    public class Scout : Character
    {
        public const string SpriteReference = "CharacterSelect/Buttons/ScoutButton";
        public const string AbilityDescription =
            "Increase the speed of all ally units on board by 4. (Units are more likely to attack/move first)";

        public Scout()
        {

        }

        public override void PerformAbility(HexGrid grid, Player player)
        {
            foreach (Unit unit in grid.Units.Where(unit => unit.OwnerId == player.Id))
            {
                unit.Stats.Speed.Augment(x => x + 4);
                unit.SetAugmentFlag();
            }
        }
    }
}