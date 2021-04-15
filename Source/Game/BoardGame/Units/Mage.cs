using Microsoft.Xna.Framework;

namespace Hex.BoardGame
{
    public class Mage : Unit
    {
        public const string SpriteReference = "Units/Mage";
        public const int Cost = 2;

        public Mage(int ownerId)
            : base(
            ownerId,
            maxHp: 85,
            baseDamage: 20,
            speed: 4,
            moveDistance: 1,
            range: 3,
            cubeCoordinate: Vector3.Zero
            )
        {

        }
    }

}
