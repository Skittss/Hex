using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Engine.GridUtils
{
    public static class Neighbours
    {
        private static Vector3[] directions = new Vector3[6]
        {
            new Vector3(1, -1, 0), new Vector3(1, 0, -1),
            new Vector3(0, 1, -1), new Vector3(-1, 1, 0),
            new Vector3(-1, 0, 1), new Vector3(0, -1, 1)
        };
            
        public static Vector3[] GetNeighbours(Vector3 cubeCoordinate, Vector3[] impassableCubeCoordinates, Vector2 gridSize)
        {
            
            List<Vector3> neighbours = new List<Vector3>();
            Vector3 neighbour;
            foreach (Vector3 direction in directions)
            {
                //check for neighbours in the "cardinal" directions of the hexagonal grid (along each of the 3 axes)
                neighbour = cubeCoordinate + direction;

                //if the neighbour is not impassable and within the grid bounds
                if (!impassableCubeCoordinates.Contains(neighbour)
                   && new Rectangle(
                        Point.Zero,
                        new Point((int)gridSize.X, (int)gridSize.Y))
                    .Contains(CoordConverter.CubeToOffset(neighbour).ToPoint()))
                {
                    neighbours.Add(neighbour);
                }
            }
            return neighbours.ToArray();
        }
    }
}
