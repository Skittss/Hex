using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Priority_Queue;
using System;

namespace Engine.GridUtils
{
    public static class AStarPathFinder
    {
        //container class for use in priority queue
        public class CubeCoordNode : FastPriorityQueueNode
        {
            public Vector3 cubeCoord;
            public CubeCoordNode(Vector3 cubeCoord)
            {
                this.cubeCoord = cubeCoord;
            }
        }


        //find a path to the nearest available point within a range (rangeLimiter) about the endCubeCoord specified.
        public static Vector3[] GetPath(Vector3 startCubeCoord, Vector3 endCubeCoord, int rangeLimiter, Vector2 gridSize, Vector3[] impassableCoords)
        {
            //Get a generated backwards-direction-path and calculated endpoint in the range specified T<path, endpoint>
            Tuple<Dictionary<Vector3, Vector3>, Vector3?> pathInfo = ConstructPath(startCubeCoord, endCubeCoord, rangeLimiter, gridSize, impassableCoords);
            if (pathInfo.Item2 != null) //if there is a path found
            {
                //reconstruct a path from the dictionary of pointed movements.
                return ReconstructPath(startCubeCoord, (Vector3)pathInfo.Item2, pathInfo.Item1);
            }
            else
            {
                //if the destination is null, a path has not been found, so return a null path.
                return null;
            }
        }

        private static Tuple<Dictionary<Vector3, Vector3>, Vector3?> ConstructPath(Vector3 startCubeCoord, Vector3 endCubeCoord, int rangeLimiter, Vector2 gridSize, Vector3[] impassableCoords)
        {
            FastPriorityQueue<CubeCoordNode> boundary = new FastPriorityQueue<CubeCoordNode>(100);
            boundary.Enqueue(new CubeCoordNode(startCubeCoord), 0); //begin the boundary at the start point
            //(Imagine this as an expanding frontier of tiles searched in the direction towards the end point)
            Dictionary<Vector3, Vector3> path = new Dictionary<Vector3, Vector3>();
            path[startCubeCoord] = Vector3.Zero;  //the start coord does not point to anything; this can be any value
            Dictionary<Vector3, int> cost = new Dictionary<Vector3, int>();
            cost[startCubeCoord] = 0;   //initialize cost of path.
            Vector3 currentCoord;
            Vector3? calculatedDestination = null;  //this determines if a path is found to a tile in range, and is used to reconstruct a path into an array of coordinates.
            int updatedCost;

            while (!(boundary.Count == 0))
            {
                currentCoord = boundary.Dequeue().cubeCoord;

                //early exit if coordinate is found. Can be used as manhattan distance is a good heurstic,
                //  meaning the first path found should always be the shortest path.
                if (CoordRange.CoordInRange(currentCoord, endCubeCoord, rangeLimiter))
                {
                    calculatedDestination = currentCoord;   //path found ending in range.
                    break;
                }

                foreach (Vector3 neighbour in Neighbours.GetNeighbours(currentCoord, impassableCoords, gridSize))
                {
                    updatedCost = cost[currentCoord] + 1;   //calculate a new cost for the current path. (add 1 as movements are of equal value on any tile)
                    if (!cost.ContainsKey(neighbour) || updatedCost < cost[neighbour])
                    {
                        cost[neighbour] = updatedCost;
                        int priority = updatedCost + ManhattanDistance.GetDistance(neighbour, endCubeCoord);
                        boundary.Enqueue(new CubeCoordNode(neighbour), priority);
                        path[neighbour] = currentCoord;
                    }

                }
            }
            return new Tuple<Dictionary<Vector3, Vector3>, Vector3?>(path, calculatedDestination);
        }

        private static Vector3[] ReconstructPath(Vector3 startCubeCoord, Vector3 calculatedDestination, Dictionary<Vector3, Vector3> dictPath)
        {
            //convert from dictionary of directions to coord array by effectively doing the inverse
            Vector3 currentCoord = calculatedDestination;
            List<Vector3> path = new List<Vector3>();
            while (currentCoord != startCubeCoord)
            {
                path.Add(currentCoord);
                currentCoord = dictPath[currentCoord];
            }
            path.Reverse();
            return path.ToArray();

        }
    }
}