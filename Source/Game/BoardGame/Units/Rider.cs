using Microsoft.Xna.Framework;

namespace Hex.BoardGame
{
    public class Rider : Unit
    {
        public const string SpriteReference = "Units/Rider";
        public const int Cost = 3;

        public Rider(int ownerId)
            : base(
            ownerId,
            maxHp: 110,
            baseDamage: 30,
            speed: 5,
            moveDistance: 3,
            range: 1,
            cubeCoordinate: Vector3.Zero
            )
        {

        }
    }

}
