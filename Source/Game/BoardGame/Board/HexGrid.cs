using Microsoft.Xna.Framework;
using Engine.GridUtils;
using System.Collections.Generic;
using System.Linq;

namespace Hex.BoardGame
{

    /// <summary>
    /// implemented as a pointy-hex grid using cube coordinates
    /// Origin of the grid is the bottom left tile.
    /// </summary>
    public class HexGrid
    {
        private HexTile[,] tiles;
        public HexTile[,] Tiles { get { return tiles; } }

        private List<Unit> gridUnits = new List<Unit>();
        public List<Unit> Units { get { return gridUnits; } }

        private List<Unit> tempUnits = new List<Unit>();

        private List<Unit> deadUnits = new List<Unit>();

        public List<Unit> DeadUnits { get { return deadUnits; }}

        private Vector2 size;
        public Vector2 Size { get { return size; } }

        //Drawing variables.
        private static Vector2 evenToOddRowOffset = new Vector2(-1, -12);   //
        private static Vector2 oddToEvenRowOffset = new Vector2(-14, -6);   //based off of sprite data
        private static Vector2 horizontalRowOffset = new Vector2(13, -6);   //
        public static Vector2 OddToEvenRowOffset { get { return oddToEvenRowOffset; } }

        public HexGrid(int offsetSizeX, int offsetSizeY, Vector2 screenBoardOrigin, float boardScale)
        {
            size = new Vector2(offsetSizeX, offsetSizeY);
            GenerateTiles(offsetSizeX, offsetSizeY, screenBoardOrigin, boardScale);
        }

        public void AssignHalves(Player pA, Player pB)
        {
            //assign a half of the grid to each player. (where they can place units)
            //get the last row for player A's half
            int pALastRow = (int)size.Y / 2 - 1;

            pA.AssignHalf(Vector2.Zero, new Vector2(size.X - 1, pALastRow));
            pB.AssignHalf(new Vector2(0, pALastRow + 1), size - Vector2.One);
        }

        public bool IsTileOccupied(Vector3 cubeCoordinate)
        {
            Vector2 offsetCoordinate = CoordConverter.CubeToOffset(cubeCoordinate);
            return tiles[(int)offsetCoordinate.X, (int)offsetCoordinate.Y].Occupied;
        }

        public Vector3[] GetAllOccupiedTiles()
        {
            //easier to linear search this than to write custom query code
            // for a square array. (no convenient inbuilt functions?)
            List<Vector3> occupiedTiles = new List<Vector3>();
            foreach (HexTile tile in tiles)
            {
                if (tile.Occupied)
                {
                    occupiedTiles.Add(tile.CubeCoordinate);
                }
            }
            return occupiedTiles.ToArray();
        }

        public HexTile GetTileOnCoordinate(Vector3 cubeCoordinate)
        {
            //linear search...
            foreach (HexTile tile in tiles)
            {
                if (tile.CubeCoordinate == cubeCoordinate)
                {
                    return tile;
                }
            }

            //will return null if no tile found.
            return null;
        }

        public Unit GetUnitFromCoordinate(Vector3 cubeCoordinate)
        {
            //linear search units checking their coordinates.
            foreach (Unit unit in gridUnits)
            {
                if (unit.CubeCoordinate == cubeCoordinate)
                {
                    return unit;
                }
            }

            return null;
        }

        public void OccupyTile(Vector3 cubeCoordinate)
        {
            Vector2 offsetCoordinate = CoordConverter.CubeToOffset(cubeCoordinate);
            tiles[(int)offsetCoordinate.X, (int)offsetCoordinate.Y].Occupied = true;
        }

        public void UnoccupyTile(Vector3 cubeCoordinate)
        {
            Vector2 offsetCoordinate = CoordConverter.CubeToOffset(cubeCoordinate);            
            tiles[(int)offsetCoordinate.X, (int)offsetCoordinate.Y].Occupied = false;

        }

        public void AddUnit(Unit unit)
        {
            gridUnits.Add(unit);
            //Make sure to occupy the tile so that other units
            // do not move there.
            OccupyTile(unit.CubeCoordinate);
        }

        public void RemoveUnit(Unit unit)
        {
            gridUnits = gridUnits.Where(u => u.id != unit.id).ToList();
            //similarly, unoccupy when removing so the tile is freed up.
            UnoccupyTile(unit.CubeCoordinate);
        }

        public int CountPlayerUnitsOnGrid(int ownerId)
        {
            return gridUnits.Count(unit => unit.OwnerId == ownerId);
        }

        public void RemoveAllUnitsOwnedByPlayer(int ownerId)
        {
            //remove all units from main grid and dead list with specified owner id.
            foreach (Unit unit in gridUnits.Where(unit => unit.OwnerId == ownerId))
            {
                RemoveUnit(unit);
            }
            deadUnits = deadUnits.Where(unit => unit.OwnerId != ownerId).ToList();
        }

        public void MoveUnitToDeadList(Unit unit)
        {
            gridUnits.RemoveAll(match => match.id == unit.id);
            UnoccupyTile(unit.CubeCoordinate);
            deadUnits.Add(unit);
        }

        public void ClearDeadList()
        {
            deadUnits.RemoveAll(match => true);
        }

        public void ReAddTempUnits()
        {
            foreach (Unit unit in tempUnits)
            {
                AddUnit(unit);
            }
            tempUnits.RemoveAll(match => true);
        }

        public void MoveUnitsOffGridTemporarily(int ownerId)
        {
            IEnumerable<Unit> unitsToMove = gridUnits.Where(unit => unit.OwnerId == ownerId);
            tempUnits.AddRange(unitsToMove);
            RemoveAllUnitsOwnedByPlayer(ownerId);
        }

        public Vector2 GetMaskOrigin()
        {
            return new Vector2(tiles[0, (int)size.Y - 1].PixelCoordinate.X, tiles[(int)size.X - 1, (int)size.Y - 1].PixelCoordinate.Y);
        }

        private void GenerateTiles(int offsetSizeX, int offsetSizeY, Vector2 screenBoardOrigin, float boardScale)
        {
            tiles = new HexTile[offsetSizeX, offsetSizeY];
            //generate offset grid and convert to cube coords
            for (int x = 0; x < offsetSizeX; x++)
            {
                for (int y = 0; y < offsetSizeY; y++)
                {
                    Vector2 verticalRowOffset = Vector2.Zero;
                    //calculate the rows offset from the grid draw origin. use this to calculate each tile's pixel position.
                    // (position to draw on screen so they tessalate)
                    for (int i = 0; i < y; i++)
                    {
                        if ((i & 1) == 1)   //if current row is odd
                        {
                            verticalRowOffset += oddToEvenRowOffset;
                        }
                        else
                        {
                            verticalRowOffset += evenToOddRowOffset;
                        }
                    }
                    //pixel position:
                    Vector2 pixelCoordinate = screenBoardOrigin + (((x * horizontalRowOffset) + verticalRowOffset) * boardScale);

                    tiles[x, y] = new HexTile(CoordConverter.OffsetToCube(new Vector2(x, y)), pixelCoordinate);
                }
            }
        }
    }
}