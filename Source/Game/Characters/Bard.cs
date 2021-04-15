using Hex.BoardGame;
using System.Linq;

namespace Hex.Characters
{
    public class Bard : Character
    {
        public const string SpriteReference = "CharacterSelect/Buttons/BardButton";
        public const string AbilityDescription =
            "Increase the maximum health of all ally units on board by 30%";

        public Bard()
        {

        }

        public override void PerformAbility(HexGrid grid, Player player)
        {
            foreach (Unit unit in grid.Units.Where(unit => unit.OwnerId == player.Id))
            {
                //increase the maxhp by 1.25x
                unit.Stats.MaxHp.Augment(x => (int)(x * 1.3));
                //make sure to heal to full.
                unit.RestoreToFullHp();
                unit.SetAugmentFlag();
            }
        }
    }
}
