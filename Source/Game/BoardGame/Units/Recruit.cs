using Microsoft.Xna.Framework;

namespace Hex.BoardGame
{
    public class Recruit : Unit
    {
        public const string SpriteReference = "Units/Recruit";
        public const int Cost = 4;

        public Recruit(int ownerId)
            : base(
            ownerId,
            maxHp: 135,
            baseDamage: 45,
            speed: 5,
            moveDistance: 1,
            range: 1,
            cubeCoordinate: Vector3.Zero
            )
        {

        }
    }

}
