using Microsoft.Xna.Framework;

namespace Engine.GridUtils
{
    public static class CoordConverter
    {
        public static Vector2 CubeToOffset(Vector3 cubeCoordinate)
        {
            float y = (int)cubeCoordinate.Z;
            float x = (int)cubeCoordinate.X + ((int)cubeCoordinate.Z - ((int)cubeCoordinate.Z & 1)) / 2;
            return new Vector2(x, y);
        }

        public static Vector3 OffsetToCube(Vector2 offsetCoordinate)
        {
            float x = (int)offsetCoordinate.X - ((int)offsetCoordinate.Y - ((int)offsetCoordinate.Y & 1)) / 2;
            float z = offsetCoordinate.Y;
            float y = -x - z;
            return new Vector3(x, y, z);
        }
    }
}