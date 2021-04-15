using Microsoft.Xna.Framework;

namespace Hex.BoardGame
{
    class Crab : Unit
    {
        public const string SpriteReference = "Units/CrabUnit";
        public const int Cost = 1;

        public Crab(int ownerId)
            : base(
            ownerId,
            maxHp: 40,
            baseDamage: 10,
            speed: 10,
            moveDistance: 10,
            range: 1,
            cubeCoordinate: Vector3.Zero
            )
        {

        }
    }
}
