using Microsoft.Xna.Framework;
using Engine.GridUtils;

namespace Hex.BoardGame
{
    //simple container class for tile information.
    public class HexTile
    {
        private Vector3 cubeCoordinate;
        public Vector3 CubeCoordinate { get { return cubeCoordinate; } }
        public Vector2 OffsetCoordinate { get { return CoordConverter.CubeToOffset(cubeCoordinate); } }

        private Vector2 pixelCoordinate;    //the top left of the sprite
        public Vector2 PixelCoordinate { get { return pixelCoordinate; } }

        private bool occupied = false;
        public bool Occupied { get; set; }

        private Vector2 centrePixelOffset = new Vector2(8, 5);  //from sprite data

        public HexTile(Vector3 cubeCoordinate, Vector2 pixelCoordinate)
        {
            this.cubeCoordinate = cubeCoordinate;
            this.pixelCoordinate = pixelCoordinate;
        }

        public Vector2 GetCentrePixelCoordinate(float boardScale)
        {
            return pixelCoordinate + (centrePixelOffset * boardScale);
        }

    }
}