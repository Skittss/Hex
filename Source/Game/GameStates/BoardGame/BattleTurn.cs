using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Engine.UI;
using Engine.GameStates;
using Hex.UI;
using Hex.BoardGame;
using System.Linq;
using System.Collections.Generic;
using System;
using Engine.GridUtils;

namespace Hex.GameStates
{
    public class BattleTurn : GameState
    {
        private bool FIRSTLOAD = true;
        private bool EXITING = false;

        //Containing enum & classes to hold information aboutn each step action
        public enum updateAction
        {
            None,
            Move,
            Attack
        }

        public class ActionArgs
        {
            public bool IsNone { get { return action == updateAction.None; } }
            public bool IsMove { get { return action == updateAction.Move; } }
            public bool IsAttack { get { return action == updateAction.Attack; } }

            private updateAction action;
            public readonly Vector3[] Path;
            public readonly Unit ActingUnit;
            public readonly Unit TargetUnit;
            public readonly bool TargetFainted;
            public readonly int MoveIndex;

            public ActionArgs(Unit actingUnit, Unit targetUnit, updateAction action, Vector3[] path, int moveIndex)
            {
                ActingUnit = actingUnit;
                TargetUnit = targetUnit;
                this.action = action;
                Path = path;
                MoveIndex = moveIndex;
                //if applicable, check if the targetted unit is still alive.
                TargetFainted = (targetUnit == null) ? false : !targetUnit.IsAlive();
            }
        }

        //log of the actions units have taken during the turn.
        private class ActionLog
        {
            private int capacity;
            private Queue<ActionArgs> log;
            public ActionArgs MostRecentAction { get { return (log.Count == 0)? null : log.Last(); } }
            public ActionLog(int capacity)
            {
                this.capacity = capacity;
                log = new Queue<ActionArgs>(capacity);
            }

            public void AddEntry(ActionArgs action)
            {
                if (log.Count == capacity)
                {
                    log.Dequeue();
                    log.Enqueue(action);
                }
                else
                {
                    log.Enqueue(action);
                }
            }

            public ActionArgs[] GetActionsInOrder()
            {
                //return the actions from most recent to least recent.
                return log.AsEnumerable().Reverse().ToArray();
            }
        }

        //Current step information
        private static float stepTimeInterval = 1f;
        private float timer = stepTimeInterval;
        private List<Unit> unitsToUpdate;
        private Unit pendingUpdateUnit;
        private ActionArgs pendingAction;

        //Round Information
        private int stageNumber;
        private bool useAiOpponent;
        private Unit[] aiRoundUnits;
        private int nonAiPlayerId;
        private int moneyAtRoundStart;
        private Player playerToCheckWinCondition;
        private Player otherPlayer;
        private Player[] players;

        //Grid
        private HexGrid grid;

        //Drawing variables (UI & Grid)
        private static float boardScale = WindowTools.GetUpscaledValue(5f);
        private Vector2 boardScaleVector = new Vector2(boardScale);
        private Texture2D isoTile;
        private Texture2D isoTileOutline;
        private static Texture2D emptyRect;
        private static Vector2 healthBarDimensions = new Vector2(70,6) * WindowTools.GetUpscaleRatio();

        private static float uiScale = WindowTools.GetUpscaledValue(4f);
        private static Vector2 uiScaleVector = new Vector2(uiScale);
        private Texture2D characterBoundary;
        private Texture2D augmentIcon;
        private static Vector2[] characterPos = new Vector2[]
        {
        WindowTools.PaddingToPixelCoordinate(0f, 0f, 10, 10) + new Vector2(3 * uiScale, 0),
        WindowTools.PaddingToPixelCoordinate(0f, 1f, 10, -10) + new Vector2(3 * uiScale, 0)
        };
        
        private Texture2D moneyIcon;
        private RectBox moneyUiBox;
        private Vector2 moneyBoxDimensions = new Vector2(WindowTools.GetUpscaledValue(175f), WindowTools.GetUpscaledValue(50f));
        private Vector2 winBarDimensions = new Vector2(WindowTools.GetUpscaledValue(400f), WindowTools.GetUpscaledValue(30f));
        private TutorialButton helpButton;

        //Action log
        private static int logCapacity = 8;
        private ActionLog actionLog = new ActionLog(logCapacity);  //see classes defined above.
        private ActionLogBox logDisplay;
        private ActionLogBox.DrawParams actionLogTextures;


        //Units
        private Dictionary<Type, Texture2D> unitSprites = new Dictionary<Type, Texture2D>();

        //Fonts
        private SpriteFont bebasSmall;

        public BattleTurn(GraphicsDevice graphicsDevice, HexGrid grid, Player[] players, int stageNumber, bool useAiOpponent, int nonAiPlayerId, Unit[] aiRoundUnits) : base(graphicsDevice)
        {
            this.grid = grid;
            this.stageNumber = stageNumber;
            this.useAiOpponent = useAiOpponent;
            this.nonAiPlayerId = nonAiPlayerId;
            this.aiRoundUnits = aiRoundUnits;
            this.players = players;
        }

        public override void Initialize()
        {
            //determine the player to check for round end against.
            if (useAiOpponent)
            {
                playerToCheckWinCondition = players.Where(player => player.Id == nonAiPlayerId).First();
                otherPlayer = players.Where(player => player.Id != nonAiPlayerId).First();
            }
            else
            {
                playerToCheckWinCondition = players.First();
                otherPlayer = players.Last();
            }
            //Won't be adding on start code here which changes the boards state as on a vsAI round, two battleTurns will be
            // initialized in one update cycle.
        }

        public override void LoadContent()
        {
            bebasSmall = contentManager.Load<SpriteFont>("Fonts/BebasNeue_Small");
            LoadContent_Grid(contentManager);
            LoadContent_UnitSprites(contentManager);
            LoadContent_Ui(contentManager);
        }
        private void LoadContent_Grid(ContentManager Content)
        {
            isoTile = Content.Load<Texture2D>("Board/isoTile");
            isoTileOutline = Content.Load<Texture2D>("Board/isoTileOutline");
        }

        private void LoadContent_UnitSprites(ContentManager Content)
        {
            //generate a dictionary of unit types to their sprite.
            // makes drawing each unit's sprite a lot easier
            foreach (Type unitType in UnitPool.GetUnitPool())
            {
                string directory = (string)unitType.GetField("SpriteReference").GetRawConstantValue();
                unitSprites[unitType] = Content.Load<Texture2D>(directory);
            }
        }

        private void LoadContent_Ui(ContentManager Content)
        {
            //Load icons
            augmentIcon = Content.Load<Texture2D>("PreparationTurn/StatAugmentIcon");
            characterBoundary = Content.Load<Texture2D>("CharacterSelect/Buttons/ButtonBoundaryLR");
            emptyRect = UiTools.GeneratePixelTexture(graphicsDevice);   //used to draw % bars
            moneyIcon = Content.Load<Texture2D>("PreparationTurn/MoneyIcon");

            //load money ui if vs ai
            if (useAiOpponent)
            {
                moneyUiBox = new RectBox(graphicsDevice,
                    characterPos[0] + new Vector2(uiScale * playerToCheckWinCondition.CharacterSprite.Width / 2, 0),
                    (int)moneyBoxDimensions.X, (int)moneyBoxDimensions.Y
                    );
            }

            actionLogTextures = new ActionLogBox.DrawParams(
                unitSprites,
                Content.Load<Texture2D>("BattleTurn/AttackIcon"),
                Content.Load<Texture2D>("BattleTurn/DefenseIcon"),
                Content.Load<Texture2D>("BattleTurn/NoMoveIcon"),
                Content.Load<Texture2D>("BattleTurn/Footprints"),
                Content.Load<Texture2D>("BattleTurn/SkullIcon"),
                Content.Load<Texture2D>("BattleTurn/TargetIcon")
                );

            logDisplay = new ActionLogBox(graphicsDevice, logCapacity, bebasSmall, WindowTools.PaddingToPixelCoordinate(1f, 0.5f, -10, 0));
            logDisplay.SetAnchorPoint(1f, 0.5f);

            helpButton = new TutorialButton(graphicsDevice, this,
                WindowTools.PaddingToPixelCoordinate(0.9f, 0f, 0, 10),
                (int)(20 * uiScale),
                (int)(10 * uiScale));
        }

        public override void Update(GameTime gameTime)
        {

            if (FIRSTLOAD)
            {
                //must put some first loading code here, since when states are added to the stack they have their content load/initialize functions called immediately.
                //  This would result in some strange behaviour with units being removed from the grid when they shouldn't be if this was put in load/initialize.
                if (useAiOpponent)
                {
                    //the following variable is used for displaying the outcome of the ai round at the end of the round.
                    moneyAtRoundStart = playerToCheckWinCondition.Money;

                    //remove the other player's units this turn only
                    otherPlayer = players.Where(player => player.Id != nonAiPlayerId).First();
                    grid.MoveUnitsOffGridTemporarily(otherPlayer.Id);

                    //add the ai opponents units to the grid.
                    Random random = new Random();
                    foreach (Unit unit in aiRoundUnits)
                    {
                        //find an unoccupied tile in the opponents half (Note their units have been removed temporarily this round)
                        Vector3 location = Vector3.Zero;
                        bool occupied = true;
                        while (occupied)
                        {
                            location = otherPlayer.GetRandomPositionInHalf();
                            if (!grid.IsTileOccupied(location))
                            {
                                occupied = false;
                            }
                        }
                        //add the ai unit at the random location.
                        unit.AddToGrid(grid, location);
                    }
                }
                FIRSTLOAD = false;
            }

            helpButton.Update(gameTime);

            //check that the round has not finished. i.e. only one side's units are left.
            int oneSidesUnitCount = grid.Units.Where(unit => unit.OwnerId == playerToCheckWinCondition.Id).ToList().Count;
            if (oneSidesUnitCount == 0 || oneSidesUnitCount == grid.Units.Count)
            {
                EndBattle();
            }
            else
            {
                //each time the timer resets...
                if (timer == stepTimeInterval)
                {
                    //get the next unit to update 
                    //unitsToUpdate list stores the units that haven't been updated in order by their speed for each step
                    if (unitsToUpdate == null)
                    {
                        //get a new list if its hasn't been defined
                        //(Note: order by descending, higher speed => move first)
                        unitsToUpdate = grid.Units.OrderByDescending(unit => unit.Stats.Speed.Value).ToList();

                    }
                    else if (unitsToUpdate.Count == 0)
                    {
                        //... or has been emptied.
                        unitsToUpdate = grid.Units.OrderByDescending(unit => unit.Stats.Speed.Value).ToList();

                    }

                    if (unitsToUpdate.Count != 0)
                    {
                        //first get information about the next move a unit will make so it can be displayed
                        //to the player before being committed.

                        //check that list is not empty before trying to update the unit.
                        pendingUpdateUnit = unitsToUpdate.First(); //get the fastest unit and store it to commit changes later.
                        if (pendingUpdateUnit.IsAlive())
                        {
                            pendingAction = pendingUpdateUnit.Update(players, grid, false);  //... and update it - not commiting changes (get pending action info)
                        }
                    }
                }

                //decrement the timer by time elapsed.
                timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                //commit changes if the interval has elapsed (gives time for update to be shown to player)
                if (timer <= 0)
                {
                    if (pendingUpdateUnit != null && pendingAction != null)
                    {
                        ActionArgs action = pendingUpdateUnit.Update(players, grid, true); //update the unit, comitting changes
                        actionLog.AddEntry(action); //add result of update to the action log (the action performed)

                        //then remove it from units needing to be updated for this step.
                        unitsToUpdate.RemoveAll(match => match.id == pendingUpdateUnit.id);

                        //remove any units thay may have fainted as a result of an action to stop them from trying to update this step.
                        unitsToUpdate.RemoveAll(match => match.IsAlive() == false);

                        //and finally reset the interval timer.
                        timer = stepTimeInterval;
                    }
                }
            }
        }

        private void EndBattle()
        {
            if (EXITING)
            {
                //battle-end cleanup.
                //iterate over all units involved in the battle, both dead and alive
                foreach (Unit unit in grid.Units.Concat(grid.DeadUnits).ToList())
                {
                    //restore them to full hp
                    unit.RestoreToFullHp();

                    //return them to their position on the preperation turn screen
                    //  making sure to unoccupy the tile (remove from grid first)
                    unit.RemoveFromGrid(grid);
                    unit.AddToGrid(grid, unit.StartCubeCoordinate);
                    //reset the target of all units so that they don't try to attack a unit that has been moved off the grid
                    unit.SetTargetNull();
                }
                //clear the dead unit list after readding them to the grid
                grid.ClearDeadList();
                if (useAiOpponent)
                {
                    //remove all ai units from the grid if this was an AI round.
                    grid.RemoveAllUnitsOwnedByPlayer(3);
                    //re-add the players units that were temporarily removed.
                    grid.ReAddTempUnits();
                }
                //finally, exit the round.
                GameStateManager.Instance.PopState();
            }
            else
            {
                //dont perform end cleanup here so that the
                // overlay screen draws over the last frame of the battle.
                EXITING = true;            
                string[] roundEndText;
                if (!useAiOpponent)
                {
                    int unitCount = grid.Units.Where(unit => unit.OwnerId == playerToCheckWinCondition.Id).Count();
                    Player winner = (unitCount == 0) ? players.Where(player => player.Id != playerToCheckWinCondition.Id).First()
                        : players.Where(player => player.Id == playerToCheckWinCondition.Id).First();

                    winner.IncrementWinProgress();
                    //award the winning player with money equivalent to the stage number.
                    // => later wins are more valuable.
                    winner.Money += stageNumber;

                    //check if the entire game is finished - i.e. one player
                    // hits the win threshold.
                    bool ended = CheckGameEnd();
                    if (!ended)
                    {
                        //if it hasnt finished, add the normal round end screen to the stack.
                        roundEndText = new string[]
                        {
                            $"Player {winner.Id + 1} defeated Player {players.Where(player => player.Id != winner.Id).First().Id + 1}!",
                            $"Their wins went from {winner.WinProgress - 1} to {winner.WinProgress}!"
                        };
                        GameStateManager.Instance.AddState(new RoundEndScreen(graphicsDevice, this, roundEndText));
                    }
                }
                else
                {
                    //standard round end screen for ai rounds added to the stack here.
                    // (note no check for game end as the game only ends after player vs player rounds).
                    int diff = playerToCheckWinCondition.Money - moneyAtRoundStart;
                    roundEndText = new string[]
                    {
                        $"Player {playerToCheckWinCondition.Id + 1} defeated {diff} ai unit{((diff == 1)? ' ' : 's')}",
                        (diff == 0)? $"and didnt gain any money!" : $"and gained {diff} money"
                    };
                    GameStateManager.Instance.AddState(new RoundEndScreen(graphicsDevice, this, roundEndText));
                }
            }


        }

        public bool CheckGameEnd()
        {
            bool ended = false;
            if (players[0].WinProgress == players[0].WinThreshold)
            {
                ended = true;
                //add the final game screen to the stack of states.
                GameStateManager.Instance.AddState(new GameEndScreen(graphicsDevice, this, players[0], players[1]));
            }
            else if (players[1].WinProgress == players[1].WinThreshold)
            {
                ended = true;
                //(in both cases, regardless of which player wins)
                GameStateManager.Instance.AddState(new GameEndScreen(graphicsDevice, this, players[1], players[0]));
            }
            return ended;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!EXITING || !FIRSTLOAD)
            {
                spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                graphicsDevice.Clear(Color.Orange);
                Draw_Grid(spriteBatch);            
                Draw_Units(spriteBatch);
                Draw_UnitUI(spriteBatch);
                Draw_UI(spriteBatch);
                spriteBatch.End();
            }
        }

        private void Draw_Grid(SpriteBatch spriteBatch)
        {
            //strange iteration over the grid is used to layer the textures and create
            // the isometric perspective. (from back right to bottom left)
            for (int y = (int)grid.Size.Y - 1; y > -1; y--)
            {
                for (int x = (int)grid.Size.X - 1; x > -1; x--)
                {
                    spriteBatch.Draw(isoTile, position: grid.Tiles[x, y].PixelCoordinate, scale: boardScaleVector);
                    //Draw action textures here to prevent having to iterate over the grid again.
                    Draw_StepActionMove(spriteBatch, CoordConverter.OffsetToCube(new Vector2(x,y)));
                }
            }
        }

        private void Draw_StepActionMove(SpriteBatch spriteBatch, Vector3 coord)
        {
            //Highlight tiles that are in the path the unit will take - yellow: immedaitely moves over, gray: will move to on next step unless conditions change.
            if (pendingAction != null)
            {
                if (pendingAction.IsMove)
                {
                    if (Enumerable.Range(0, pendingAction.MoveIndex + 1).Contains(Array.IndexOf(pendingAction.Path, coord)))
                    {
                        //draw tiles that are immediately moved to/over with a gold outline.
                        spriteBatch.Draw(isoTileOutline, position: grid.GetTileOnCoordinate(coord).PixelCoordinate - boardScaleVector, scale: boardScaleVector, color: Color.Gold);

                    }
                    else if (pendingAction.Path.Contains(coord))
                    {
                        //other tiles along the path are drawn with a gray outline.
                        spriteBatch.Draw(isoTileOutline, position: grid.GetTileOnCoordinate(coord).PixelCoordinate - boardScaleVector, scale: boardScaleVector, color: Color.DarkGray);
                    }
                }
            }
        }

        private void Draw_Units(SpriteBatch spriteBatch)
        {
            //draw units on the board - Ordering by primarily Z coordinate then by Y
            //  ensures units are drawn cascading from the top right to bottom left, and the perspective is maintained
            foreach (Unit unit in grid.Units.OrderByDescending(i => i.CubeCoordinate.Z).ThenBy(i => i.CubeCoordinate.Y))
            {
                Vector2 tileCoordinate = CoordConverter.CubeToOffset(unit.CubeCoordinate);
                Texture2D unitTexture = unit.GetSprite(unitSprites);
                Vector2 pos = grid.Tiles[(int)tileCoordinate.X, (int)tileCoordinate.Y].GetCentrePixelCoordinate(boardScale) - new Vector2(boardScale * (unitTexture.Width / 2), unitTexture.Height * boardScale);
                spriteBatch.Draw(unitTexture, position: pos, scale: boardScaleVector);
                Draw_StepActionOverUnits(spriteBatch, unit, pos);
            }
        }

        private void Draw_StepActionOverUnits(SpriteBatch spriteBatch, Unit unit, Vector2 unitDrawPos)
        {
            //Draw information about what action a unit is about to take - specifically textures drawn OVER units
            if (pendingAction != null)
            {
                if (pendingAction.IsAttack)
                {
                    //draw attack information: sword slash over attacking unit, shield over defending unit.
                    if (pendingAction.ActingUnit == unit)
                    {
                        spriteBatch.Draw(actionLogTextures.attackIcon,
                            position: unitDrawPos + boardScale / 2 * (UiTools.BoundsToVector(unit.GetSprite(unitSprites).Bounds) - UiTools.BoundsToVector(actionLogTextures.attackIcon.Bounds)),
                            scale: boardScaleVector
                            );
                    }
                    else if (pendingAction.TargetUnit == unit)
                    {
                        spriteBatch.Draw(actionLogTextures.defenseIcon,
                            position: unitDrawPos + boardScale / 2 * (UiTools.BoundsToVector(unit.GetSprite(unitSprites).Bounds) - UiTools.BoundsToVector(actionLogTextures.defenseIcon.Bounds)),
                            scale: boardScaleVector
                            );
                    }
                }
                else if (pendingAction.IsMove)
                {
                    //draw move information: target over the unit the moving unit is going towards.
                    if (pendingAction.ActingUnit == unit)
                    {

                    }
                    else if (pendingAction.TargetUnit == unit)
                    {
                        spriteBatch.Draw(actionLogTextures.targetIcon,
                            position: unitDrawPos + boardScale / 2 * (UiTools.BoundsToVector(unit.GetSprite(unitSprites).Bounds) - UiTools.BoundsToVector(actionLogTextures.targetIcon.Bounds)),
                            scale: boardScaleVector
                            );
                    }
                }
                else if (pendingAction.IsNone)
                {
                    //draw a big red cross! This unit can't perform an action in its current position.
                    if (pendingAction.ActingUnit == unit)
                    {
                        spriteBatch.Draw(actionLogTextures.noMoveIcon,
                            position: unitDrawPos + boardScale / 2 * (UiTools.BoundsToVector(unit.GetSprite(unitSprites).Bounds) - UiTools.BoundsToVector(actionLogTextures.noMoveIcon.Bounds)),
                            scale: boardScaleVector
                            );
                    }
                }
            }

        }

        private void Draw_UnitUI(SpriteBatch spriteBatch)
        {
            //need a seperate loop here to prevent units from being drawn over bars.
            foreach (Unit unit in grid.Units)
            {
                //draw health bars
                Vector2 tileCoordinate = CoordConverter.CubeToOffset(unit.CubeCoordinate);
                Texture2D unitTexture = unitSprites[unit.GetType()];
                Vector2 pos = grid.Tiles[(int)tileCoordinate.X, (int)tileCoordinate.Y].GetCentrePixelCoordinate(boardScale) - new Vector2(boardScale * (unitTexture.Width / 2), unitTexture.Height * boardScale);
                Color barColor = (players.Where(player => player.Id == unit.OwnerId).ToArray().Length == 0) ? Color.Gray : players.Where(player => player.Id == unit.OwnerId).First().Colour;
                PercentageBar.Draw(spriteBatch, emptyRect, pos - new Vector2(0, 15) * WindowTools.GetUpscaleRatio(), healthBarDimensions, barColor, Color.OrangeRed, unit.Stats.MaxHp.Value, unit.Hp);
                //draw an icon to show the unit has augmented stats if it does.
                // (these units are strong!)
                if (unit.Augmented)
                {
                    spriteBatch.Draw(augmentIcon,
                        position: pos - new Vector2(0, 15) - uiScale/2 * new Vector2(augmentIcon.Width, augmentIcon.Height/2),
                        scale: uiScaleVector/2);
                }
            }
        }

        private void Draw_UI(SpriteBatch spriteBatch)
        {
            helpButton.Draw(spriteBatch, bebasSmall, 1f);

            //draw the players character and money if vs an AI.
            if (useAiOpponent)
            {
                moneyUiBox.Draw(spriteBatch);
                string moneyText = $"{playerToCheckWinCondition.Money}";
                Vector2 textSize = bebasSmall.MeasureString(moneyText);
                Vector2 basePos = moneyUiBox.GetPos() + new Vector2(moneyBoxDimensions.X / 2f, moneyBoxDimensions.Y / 2);
                spriteBatch.DrawString(bebasSmall, moneyText, basePos - new Vector2(0, textSize.Y / 2), Color.White);
                spriteBatch.Draw(moneyIcon,
                    position: basePos + new Vector2(textSize.X + moneyBoxDimensions.X / 35, 0) - uiScale / 2 * new Vector2(0, moneyIcon.Height),
                    scale: uiScaleVector
                    );

                //Draw the player's character.
                spriteBatch.Draw(texture: characterBoundary, position: characterPos[0] - new Vector2(3 * uiScale, 0), scale: uiScaleVector, color: playerToCheckWinCondition.Colour);
                spriteBatch.Draw(texture: playerToCheckWinCondition.CharacterSprite, position: characterPos[0], scale: uiScaleVector);

            }
            //otherwise, draw each players character and their win progress.
            else
            {
                //Draw each players win % bars.
                Vector2 barPos = characterPos[0] + new Vector2(uiScale * otherPlayer.CharacterSprite.Width / 2, 0f);
                PercentageBar.Draw(spriteBatch, emptyRect, barPos, winBarDimensions, otherPlayer.Colour, Color.Gray, otherPlayer.WinThreshold, otherPlayer.WinProgress);
                string text = $"{otherPlayer.WinProgress}/{otherPlayer.WinThreshold} Wins";
                spriteBatch.DrawString(bebasSmall, text, barPos + (winBarDimensions - 0.6f * bebasSmall.MeasureString(text)) / 2, Color.White, 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);

                Vector2 lowerBarPos = characterPos[1] + new Vector2(uiScale * playerToCheckWinCondition.CharacterSprite.Width / 2, -winBarDimensions.Y);
                PercentageBar.Draw(spriteBatch, emptyRect, lowerBarPos, winBarDimensions, playerToCheckWinCondition.Colour, Color.Gray, playerToCheckWinCondition.WinThreshold, playerToCheckWinCondition.WinProgress);
                text = $"{playerToCheckWinCondition.WinProgress}/{playerToCheckWinCondition.WinThreshold} Wins";
                spriteBatch.DrawString(bebasSmall, text, lowerBarPos + (winBarDimensions - 0.6f * bebasSmall.MeasureString(text)) / 2, Color.White, 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);

                //Draw each players character with their boundary (their respective colour)
                spriteBatch.Draw(texture: characterBoundary, position: characterPos[0] - new Vector2(3 * uiScale, 0), scale: uiScaleVector, color: otherPlayer.Colour);
                spriteBatch.Draw(texture: otherPlayer.CharacterSprite, position: characterPos[0], scale: uiScaleVector);

                Vector2 lowerDrawPos = characterPos[1] - new Vector2(0, playerToCheckWinCondition.CharacterSprite.Height * uiScale);
                spriteBatch.Draw(texture: characterBoundary, position: lowerDrawPos - new Vector2(3 * uiScale, 0), scale: uiScaleVector, color: playerToCheckWinCondition.Colour);
                spriteBatch.Draw(texture: playerToCheckWinCondition.CharacterSprite, position: lowerDrawPos, scale: uiScaleVector);

            }

            logDisplay.Draw(spriteBatch, actionLog.GetActionsInOrder(), players, actionLogTextures);

        }
    }
}
