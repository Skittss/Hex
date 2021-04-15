 using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.GridUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using Hex.GameStates;

namespace Hex.BoardGame
{
    public abstract class Unit
    {
        public readonly int id;
        public UnitStats Stats;
        //contain unit stats within a struct, make them able to be changed by
        // a specified function (as argument) using the defined Stat.Augment() function.

        public struct UnitStats
        {
            public struct Stat<T>
            {
                private T value;
                public T Value { get { return value; } }

                public Stat(T value)
                {
                    this.value = value;
                }

                public void Augment(Func<T, T> function)
                {
                    //apply lambda expression/function to the stats value
                    value = function(value);
                }
            }
            public Stat<int> MaxHp;
            public Stat<int> BaseDamage;
            public Stat<int> Speed;
            public Stat<int> MoveDistance;
            public Stat<int> Range;

            public UnitStats(int maxHp, int baseDamage, int speed, int moveDistance, int range)
            {
                MaxHp = new Stat<int>(maxHp);
                BaseDamage = new Stat<int>(baseDamage);
                Speed = new Stat<int>(speed);
                MoveDistance = new Stat<int>(moveDistance);
                Range = new Stat<int>(range);
            }
        }

        //holds if any of the units stats have been augmented.
        // used for drawing.
        protected bool augmented = false;
        public bool Augmented { get { return augmented; } }

        protected int hp;
        public int Hp { get { return hp; } }

        protected Unit target;

        protected bool onBench;
        public bool OnBench { get; set; }

        protected int ownerId;
        public int OwnerId { get { return ownerId; } }

        protected int benchPosition;
        public int BenchPosition
        {
            get { return benchPosition; }
            set { benchPosition = value; }
        }

        protected Vector3 startCubeCoordinate; //what tile the unit starts on
        public Vector3 StartCubeCoordinate { get { return startCubeCoordinate; } }

        protected Vector3 cubeCoordinate;
        public Vector3 CubeCoordinate
        {
            get { return cubeCoordinate; }
            set { cubeCoordinate = value; }
        }

        protected Vector2 offsetCoordinate { get { return CoordConverter.CubeToOffset(cubeCoordinate); } }

        public Unit(int ownerId, int maxHp, int baseDamage, int speed, int moveDistance, int range, Vector3 cubeCoordinate)
        {
            //lease an id to the unit.
            id = UnitIdLeaser.Instance.GenerateNewId();

            //set the units stats
            Stats = new UnitStats(
                maxHp,
                baseDamage,
                speed,
                moveDistance,
                range
                );

            //the ID of the owner is stored. this way, we can pass in a specified id to flag
            // AI units, instead of having to instantiate another player class.
            this.ownerId = ownerId;
            this.cubeCoordinate = cubeCoordinate;

            hp = maxHp;
            target = null;
        }

        public void Delete(HexGrid grid, Player owner)
        {
            //return the unit's leased id back if it is removed.
            UnitIdLeaser.Instance.ReturnId(id);
            if (onBench)
            {
                RemoveFromBench(owner);
            }
            else
            {
                RemoveFromGrid(grid);
            }
        }

        public BattleTurn.ActionArgs Update(Player[] players, HexGrid grid, bool commitUpdate)
        {
            //only update if the unit is alive.
            if (IsAlive())
            {
                //get all other units that not owned by this units owner
                Unit[] potentialTargets = grid.Units.Where(unit => unit.OwnerId != ownerId && unit.IsAlive()).ToArray();
                //choose a target from them.
                ChooseTarget(potentialTargets);
                if (target != null)
                {
                    BattleTurn.ActionArgs actionDone = PerformAction(players, grid, commitUpdate);
                    return actionDone;
                }
            }

            return new BattleTurn.ActionArgs(this, null, BattleTurn.updateAction.None, null, -1);

        }

        protected void ChooseTarget(Unit[] potentialTargets)
        {
            if (potentialTargets.Length > 0)
            {
                //order units by their manhattan distance from the units coord.
                IEnumerable<Unit> iter = potentialTargets.OrderBy(unit => ManhattanDistance.GetDistance(cubeCoordinate, unit.CubeCoordinate));
                //choose the lowest distance (first item)
                Unit closestTarget = iter.First();

                //choose the closest unit as the target if
                //  1. unit currently has no target
                //  OR 2. target has died
                //  OR 3. current target is further away than the closest potential target
                if (target == null 
                    || !target.IsAlive() 
                    || ManhattanDistance.GetDistance(cubeCoordinate, closestTarget.CubeCoordinate)
                        < ManhattanDistance.GetDistance(cubeCoordinate, target.CubeCoordinate))
                {
                    target = iter.First();
                }
                
            }
        }

        protected BattleTurn.ActionArgs PerformAction(Player[] players, HexGrid grid, bool commitAction)
        {
            //if in range of target => attack
            if (CoordRange.CoordInRange(cubeCoordinate, target.CubeCoordinate, Stats.Range.Value))
            {
                if (commitAction) { BasicAttack(players, target, grid); }
                return new BattleTurn.ActionArgs(this, target, BattleTurn.updateAction.Attack, null, -1);
            }
            //if out of range => try to move into range, move by step distance in unit stats.
            else
            {
                //follow shortest path if possible.
                Vector3[] path = AStarPathFinder.GetPath(cubeCoordinate, target.CubeCoordinate, Stats.Range.Value, grid.Size, grid.GetAllOccupiedTiles());
                if (!(path == null))
                {
                    //calculate the index of the path to move to depending on move distance.
                    // i.e. a unit with move distance 2 can skip the first location on the path,
                    // and move to the second in one update call.
                    int moveIndex = (Stats.MoveDistance.Value > path.Length) ? path.Length - 1 : Stats.MoveDistance.Value - 1;
                    Vector3 stepDestination = path[moveIndex];
                    if (commitAction) { MoveToTile(grid, stepDestination); }
                    //return action log data about what move the unit is making i.e. (this unit is acting, its moving, along path specified)...
                    return new BattleTurn.ActionArgs(this, target, BattleTurn.updateAction.Move, path, moveIndex);
                }
                //if no possible path is found, do nothing - there is no possible move.
                return new BattleTurn.ActionArgs(this, null, BattleTurn.updateAction.None, null, -1);

            }
        }

        public void SetTargetNull()
        {
            target = null;
        }

        protected void BasicAttack(Player[] players, Unit target, HexGrid grid)
        {
            //Deal damage to a target unit.
            target.InflictDamage(Stats.BaseDamage.Value, grid);
            //if the target is defeated and is ai owned, give money to the player.
            if (!target.IsAlive() && target.OwnerId == 3)
            {
                players.Where(player => player.Id == ownerId).First().Money += 1;
            }
        }

        public void InflictDamage(int damage, HexGrid grid)
        {
            hp -= damage;
            if (!IsAlive())
            {
                //Move the unit to the dead unit list if it dies.
                grid.MoveUnitToDeadList(this);
            }
        }

        public void RestoreToFullHp()
        {
            hp = Stats.MaxHp.Value;
        }

        public bool IsAlive()
        {
            if (hp <= 0)
            {
                return false;
            }
            return true;
        }

        public void AddToBench(Player player)
        {
            //extra function definition which puts the unit in the next available slot
            // when adding to the bench (useful for when buying units from the shop)
            int position = player.AddUnitToBench(this);
            benchPosition = position;
            onBench = true;
        }

        public void AddToBench(Player player, int position)
        {
            player.AddUnitToBench(this, position);
            benchPosition = position;
            onBench = true;
        }

        public void RemoveFromBench(Player player)
        {
            player.RemoveUnitFromBench(this);
            onBench = false;
        }

        public void AddToGrid(HexGrid grid, Vector3 cubeCoordinateDestination)
        {
            cubeCoordinate = startCubeCoordinate = cubeCoordinateDestination;
            grid.AddUnit(this);
        }

        public void RemoveFromGrid(HexGrid grid)
        {
            grid.RemoveUnit(this);
        }

        protected void MoveToTile(HexGrid grid, Vector3 cubeCoordDestination)
        {
            if (!grid.IsTileOccupied(cubeCoordDestination))
            {
                //unoccupy the current tile its on.
                grid.UnoccupyTile(cubeCoordinate);
                //occupy the destination tile.
                grid.OccupyTile(cubeCoordDestination);
                //update position.
                cubeCoordinate = cubeCoordDestination;
            }
        }

        public Texture2D GetSprite(Dictionary<Type, Texture2D> unitSprites)
        {
            return unitSprites[this.GetType()];
        }

        public void SetAugmentFlag()
        {
            augmented = true;
        }
    }
}
