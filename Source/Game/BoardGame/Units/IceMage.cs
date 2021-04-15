using Microsoft.Xna.Framework;

namespace Hex.BoardGame
{
    public class IceMage : Unit
    {
        public const string SpriteReference = "Units/IceMage";
        public const int Cost = 3;

        public IceMage(int ownerId)
            : base(
            ownerId,
            maxHp: 100,
            baseDamage: 30,
            speed: 4,
            moveDistance: 1,
            range: 2,
            cubeCoordinate: Vector3.Zero
            )
        {

        }
    }

}
