using Microsoft.Xna.Framework;
using System;

namespace Engine.GridUtils
{
    public static class ManhattanDistance
    {
        public static int GetDistance(Vector3 pointA, Vector3 pointB)
        {
            int distance = (int)(Math.Abs(pointA.X - pointB.X) + Math.Abs(pointA.Y - pointB.Y) + Math.Abs(pointA.Z - pointB.Z)) / 2;
            return distance;
        }
    }
}
