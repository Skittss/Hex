using Microsoft.Xna.Framework;

namespace Engine.GridUtils
{
    public static class CoordRange
    {
        public static bool CoordInRange(Vector3 pointA, Vector3 pointB, int range)
        {
            //use manhattan distance - within n steps of point?
            if (ManhattanDistance.GetDistance(pointA, pointB) <= range)
            {
                return true;
            }
            return false;
        }

    }
}
