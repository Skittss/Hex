using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Engine.Input;
using Hex.BoardGame;
using Hex.UI;
using Engine.UI;
using Engine.GameStates;
using Engine.GridUtils;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Hex.GameStates
{
    class PreparationTurn : GameState
    {
        //This turn's information
        private Player turnPlayer;
        private int stageNumber;
        private int stageIndex;
        private Stage currentStage;

        //Top Left UI
        private static float uiScale = WindowTools.GetUpscaledValue(4f);
        private Vector2 uiScaleVector = new Vector2(uiScale);
        private Vector2 characterDrawPos = WindowTools.PaddingToPixelCoordinate(0f, 0f, 10, 10) + new Vector2(3 * uiScale, 0);
        private Vector2 winBarDimensions = new Vector2(WindowTools.GetUpscaledValue(400f) , WindowTools.GetUpscaledValue(30f));
        private Vector2 unitBarDimensions = new Vector2(WindowTools.GetUpscaledValue(400f), WindowTools.GetUpscaledValue(20f));
        private RectBox moneyBox;
        private Vector2 moneyBoxDimensions = new Vector2(WindowTools.GetUpscaledValue(175f), WindowTools.GetUpscaledValue(50f));
        private Texture2D characterBoundary;
        private SinglePressSpriteButton endTurnButton;
        private LevelUnitCapButton levelUnitCapButton;
        private UseAbilityButton useAbilityButton;
        private TutorialButton helpButton;
        private Line divisorLine;
        private StageViewBox stageInfo;
        private Texture2D vsAiIcon;
        private Texture2D vsPlayerIcon;
        private Texture2D moneyIcon;
        private Texture2D augmentIcon;
        private Texture2D emptyRect;

        //Board Position & Scale
        private static Vector2 screenBoardOrigin = WindowTools.PaddingToPixelCoordinate(0.33f, 0.70f, 0, 0);    //bottom left of grid, top left of the sprite
        private static float boardScale = WindowTools.GetUpscaledValue(5f);
        private Vector2 boardScaleVector = new Vector2(boardScale);

        //Units
        private Dictionary<Type, Texture2D> unitSprites = new Dictionary<Type, Texture2D>();

        //Grid & Bench
        private Texture2D isoTile;
        private Texture2D hoveredIsoTile;
        private Texture2D isoGridMask;
        private Color[,] isoGridMaskPixelData;
        private Vector2 gridMaskOrigin;
        private HexGrid grid;

        private HexGrid bench;
        private Texture2D benchMask;
        private Color[,] benchMaskPixelData;
        private Vector2 benchMaskOrigin;

        //Tile Hovering and pickup.
        private Tuple<Vector2, bool, bool> hoveredTileData = new Tuple<Vector2, bool, bool>(Vector2.Zero, false, false); //stores coordinate, tile is hovered (not default 0), bench flag
        private Tuple<Vector2, bool, bool> pickedUpTileData = new Tuple<Vector2, bool, bool>(Vector2.Zero, false, false); //stores coord, tile has unit, bench flag

        //Fonts
        private SpriteFont bebasSmall;

        public PreparationTurn(GraphicsDevice graphicsDevice, Player turnPlayer, HexGrid grid, int stageNumber, int stageIndex, Stage currentStage) : base(graphicsDevice)  //end bit here calls the base constructor.
        {
            this.turnPlayer = turnPlayer;
            this.stageNumber = stageNumber;
            this.stageIndex = stageIndex;
            this.grid = grid;
            this.currentStage = currentStage;
        }

        public override void Initialize()
        {
            //reset the shop at the start of the prep turn.
            // this happens for free at the start of each prep turn once.
            turnPlayer.Shop.Reset();
        }

        public override void LoadContent()
        {
            bebasSmall = contentManager.Load<SpriteFont>("Fonts/BebasNeue_Small");
            LoadContent_Grid(contentManager);
            LoadContent_Bench(contentManager);
            LoadContent_UnitSprites(contentManager);
            LoadContent_Ui(contentManager);
        }

        private void LoadContent_Ui(ContentManager Content)
        {
            augmentIcon = Content.Load<Texture2D>("PreparationTurn/StatAugmentIcon");

            moneyBox = new RectBox(graphicsDevice,
                characterDrawPos + new Vector2(uiScale * turnPlayer.CharacterSprite.Width / 2, winBarDimensions.Y + unitBarDimensions.Y),
                (int)moneyBoxDimensions.X, (int)moneyBoxDimensions.Y);

            emptyRect = UiTools.GeneratePixelTexture(graphicsDevice);
            characterBoundary = Content.Load<Texture2D>("CharacterSelect/Buttons/ButtonBoundaryLR");
            vsAiIcon = Content.Load<Texture2D>("PreparationTurn/vsPlayerIcon");
            vsPlayerIcon = Content.Load<Texture2D>("PreparationTurn/vsPlayerIcon");
            moneyIcon = Content.Load<Texture2D>("PreparationTurn/MoneyIcon");
            //information used by the stage box:
            stageInfo = new StageViewBox(graphicsDevice,
                currentStage,
                bebasSmall,
                stageNumber + 1,
                stageIndex + 1,
                WindowTools.PaddingToPixelCoordinate(0f, 0.25f, 10, 10)
                );

            //Buttons
            Texture2D buttonTexture = Content.Load<Texture2D>("PreparationTurn/EndTurnButton");
            Texture2D hoverTexture = Content.Load<Texture2D>("PreparationTurn/EndTurnButtonHovered");
            bool[,] mask = UiTools.CreateBoolMask(buttonTexture);
            endTurnButton = new SinglePressSpriteButton(WindowTools.PaddingToPixelCoordinate(0f, 0.17f, 10, 10),
                buttonTexture,
                hoverTexture,
                mask, uiScale);

            //assign the end turn function to the button's event handler.
            endTurnButton.buttonPressed += HandleEndTurnButtonPress;

            levelUnitCapButton = new LevelUnitCapButton(graphicsDevice,
                moneyBox.GetPos() + new Vector2(moneyBoxDimensions.X, 0f),
                (int)WindowTools.GetUpscaledValue(225f), (int)moneyBoxDimensions.Y);

            //and so on with all used buttons
            levelUnitCapButton.buttonPressed += HandleLevelUpCapPress;

            useAbilityButton = new UseAbilityButton(graphicsDevice,
                WindowTools.PaddingToPixelCoordinate(0.1f, 0.17f, 10, 10),
                (int)WindowTools.GetUpscaledValue(225f),
                (int)(buttonTexture.Height * uiScale)
                );

            useAbilityButton.buttonPressed += HandleAbilityButtonPress;

            //divisor line to distinguish between money box/level up button.
            divisorLine = new Line(graphicsDevice, levelUnitCapButton.GetPos(), levelUnitCapButton.GetPos() + new Vector2(0, levelUnitCapButton.GetHeight()), 5, Color.White);


            //help button
            helpButton = new TutorialButton(graphicsDevice, this,
                WindowTools.PaddingToPixelCoordinate(0.90f, 0f, 0, 10),
                (int)(20 * uiScale),
                (int)(10 * uiScale));
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

        private void LoadContent_Grid(ContentManager Content)
        {
            isoTile = Content.Load<Texture2D>("Board/isoTile");
            hoveredIsoTile = Content.Load<Texture2D>("Board/hoveredIsoTile");

            isoGridMask = Content.Load<Texture2D>("Board/isoGridMask");
            isoGridMaskPixelData = UiTools.CreateColorMask(isoGridMask); //pixelData;

            gridMaskOrigin = grid.GetMaskOrigin();
        }

        private void LoadContent_Bench(ContentManager Content)
        {
            benchMask = Content.Load<Texture2D>("Bench/benchGridMask");
            benchMaskPixelData = UiTools.CreateColorMask(benchMask);

            //bench uses existing hexgrid class to attain same appearance - structure is handled within the player class.
            bench = new HexGrid(turnPlayer.GetBenchUnits().Length, 1, screenBoardOrigin - HexGrid.OddToEvenRowOffset * boardScaleVector + 5 * Vector2.One * boardScaleVector, boardScale);

            benchMaskOrigin = bench.GetMaskOrigin();
        }

        private void HandleAbilityButtonPress(object sender, SinglePressEventArgs eventArgs)
        {
            turnPlayer.PerformAbility(grid);
        }

        private void HandleLevelUpCapPress(object sender, SinglePressEventArgs eventArgs)
        {
            turnPlayer.IncreaseUnitCap();
        }

        private void HandleEndTurnButtonPress(object sender, SinglePressEventArgs eventArgs)
        {
            //end turn sanity check: player MUST have a unit on board. if there isn't one, put one on the board.
            //the player should ALWAYS have AT LEAST one unit.
            if (grid.CountPlayerUnitsOnGrid(turnPlayer.Id) == 0)
            {
                Unit unitToMove = turnPlayer.GetBenchUnits().Where(unit => unit != null).First();
                Vector3 location = turnPlayer.GetRandomPositionInHalf();
                unitToMove.RemoveFromBench(turnPlayer);
                unitToMove.AddToGrid(grid, location);
            }
            
            
            GameStateManager.Instance.PopState();
        }

        public override void Update(GameTime gameTime)
        {
            helpButton.Update(gameTime);
            endTurnButton.Update(gameTime);
            levelUnitCapButton.Update(gameTime);
            useAbilityButton.Update(gameTime);
            if (InputManager.Instance.IsKeyToggled(Keybinds.Actions.Refresh))
            {
                turnPlayer.Shop.Refresh();
            }
            turnPlayer.Shop.Update(gameTime);
            GetHoveredTile();
            ProcessTilePicked();

        }

        private void GetHoveredTile()
        {
            bool mainTileHovered = true;

            //first check if in the main grid
            //note this is the index of the 2d array, so each must be decremented by 1
            Vector2 gridMaskPixelPosition = (InputManager.Instance.MousePos - gridMaskOrigin) / boardScale;

            //if in bounds of grid mask:
            if (isoGridMask.Bounds.Contains(new Point((int)gridMaskPixelPosition.X, (int)gridMaskPixelPosition.Y)))
            {
                if (isoGridMaskPixelData[(int)gridMaskPixelPosition.X, (int)gridMaskPixelPosition.Y].A == 0)
                {
                    mainTileHovered = false;
                    //set hovered tile to no tile found flag.
                    hoveredTileData = new Tuple<Vector2, bool, bool>(Vector2.Zero, false, false);
                }
                else
                {
                    //set hovered tile to main tile found.
                    Vector2 hoveredTileCoordinate = new Vector2(isoGridMaskPixelData[(int)gridMaskPixelPosition.X, (int)gridMaskPixelPosition.Y].R, isoGridMaskPixelData[(int)gridMaskPixelPosition.X, (int)gridMaskPixelPosition.Y].G);
                    hoveredTileData = new Tuple<Vector2, bool, bool>(hoveredTileCoordinate, true, false);
                }
            }
            else
            {
                mainTileHovered = false;
                //set hovered tile to no tile found flag.
                hoveredTileData = new Tuple<Vector2, bool, bool>(Vector2.Zero, false, false);

            }

            //check bench mask if hovered tile not found in main grid.
            if (!mainTileHovered)
            {
                Vector2 benchMaskPixelPosition = (InputManager.Instance.MousePos - benchMaskOrigin) / boardScale;
                if (benchMask.Bounds.Contains(new Point((int)benchMaskPixelPosition.X, (int)benchMaskPixelPosition.Y)))
                {
                    if (benchMaskPixelData[(int)benchMaskPixelPosition.X, (int)benchMaskPixelPosition.Y].A == 0)
                    {
                        //set hovered tile to no tile found flag.
                        hoveredTileData = new Tuple<Vector2, bool, bool>(Vector2.Zero, false, false);

                    }
                    else
                    {
                        //set hovered tile to bench tile found.
                        Vector2 hoveredTileCoordinate = new Vector2(benchMaskPixelData[(int)benchMaskPixelPosition.X, (int)benchMaskPixelPosition.Y].R, 0);
                        hoveredTileData = new Tuple<Vector2, bool, bool>(hoveredTileCoordinate, true, true);
                    }
                }
                else
                {
                    //set hovered tile to no tile found flag.
                    hoveredTileData = new Tuple<Vector2, bool, bool>(Vector2.Zero, false, false);
                }
            }
        }

        private void ProcessTilePicked()
        {
            //if left click && hovering a tile
            if (InputManager.Instance.IsLeftMouseToggled() && hoveredTileData.Item2)
            {
                //if another tile has already been picked
                if (pickedUpTileData.Item2)
                {
                    //if tile is in player's half
                    if (turnPlayer.IsPointInHalf(hoveredTileData.Item1, hoveredTileData.Item3))
                    {
                        //if pending tile has a unit
                        bool hasUnit = DoesUnitExistAtDataPoint(hoveredTileData);

                        if (hasUnit)
                        {
                            Unit unitA = GetUnitFromTileData(hoveredTileData);
                            Unit unitB = GetUnitFromTileData(pickedUpTileData);

                            //if both units are on grid
                            if (!pickedUpTileData.Item3 && !hoveredTileData.Item3)
                            {
                                //swap unit positions
                                var temp = unitA.CubeCoordinate;
                                unitA.CubeCoordinate = unitB.CubeCoordinate;
                                unitB.CubeCoordinate = temp;
                            }
                            //if both units are on the board
                            else if (pickedUpTileData.Item3 && hoveredTileData.Item3)
                            {
                                //swap both units on the bench
                                turnPlayer.SwapTwoBenchUnits(unitA, unitB);
                            }
                            else if (pickedUpTileData.Item3 ^ hoveredTileData.Item3)
                            {
                                //if tiles are bench/grid (xor) swap them.
                                //get which unit is on which grid/bench
                                Unit benchUnit = hoveredTileData.Item3 ? unitA : unitB;
                                Unit gridUnit = !hoveredTileData.Item3 ? unitA : unitB;
                                //remove units from current position
                                gridUnit.RemoveFromGrid(grid);
                                benchUnit.RemoveFromBench(turnPlayer);
                                //add units back in swapped position
                                benchUnit.AddToGrid(grid, gridUnit.CubeCoordinate);
                                gridUnit.AddToBench(turnPlayer, benchUnit.BenchPosition);
                            }
                        }
                        //if there is no unit on the pending tile
                        else
                        {
                            Unit unit = GetUnitFromTileData(pickedUpTileData);
                            //if selected unit is on the grid
                            if (!pickedUpTileData.Item3)
                            {
                                unit.RemoveFromGrid(grid);

                                //if destination is bench
                                if (hoveredTileData.Item3)
                                {
                                    unit.AddToBench(turnPlayer, (int)hoveredTileData.Item1.X);
                                }

                                //if destination is grid
                                else
                                {
                                    unit.AddToGrid(grid, CoordConverter.OffsetToCube(hoveredTileData.Item1));
                                }
                            }
                            //if selected unit in on the bench
                            else
                            {
                                //if destination is bench
                                if (hoveredTileData.Item3)
                                {
                                    unit.RemoveFromBench(turnPlayer);
                                    unit.AddToBench(turnPlayer, (int)hoveredTileData.Item1.X);
                                }
                                //if destination is grid
                                else
                                {
                                    //only move a unit from the bench to the grid if the player does not have too many units
                                    if (grid.CountPlayerUnitsOnGrid(turnPlayer.Id) < turnPlayer.UnitCap)
                                    {
                                        unit.RemoveFromBench(turnPlayer);
                                        unit.AddToGrid(grid, CoordConverter.OffsetToCube(hoveredTileData.Item1));
                                    }
                                }
                            }
                        }
                        //un-pick tile (set false flag)
                        pickedUpTileData = new Tuple<Vector2, bool, bool>(Vector2.Zero, false, false);
                    }
                }
                else
                {
                    //if the sell hotkey is held and eligible unit to sell is on tile, sell it.
                    // (unit cannot be last unit a player owns)
                    if (InputManager.Instance.IsKeyPressed(Keybinds.Actions.SellUnit) &&
                        grid.CountPlayerUnitsOnGrid(turnPlayer.Id) + turnPlayer.GetBenchCount() > 1 &&
                        DoesUnitExistAtDataPoint(hoveredTileData) && turnPlayer.IsPointInHalf(hoveredTileData.Item1, hoveredTileData.Item3))
                    {
                        Unit unit = GetUnitFromTileData(hoveredTileData);
                        turnPlayer.Money += (int)unit.GetType().GetField("Cost").GetRawConstantValue();
                        unit.Delete(grid, turnPlayer);
                    }

                    //pick up tile clicked if there is a unit
                    pickedUpTileData = new Tuple<Vector2, bool, bool>(hoveredTileData.Item1, DoesUnitExistAtDataPoint(hoveredTileData) && turnPlayer.IsPointInHalf(hoveredTileData.Item1, hoveredTileData.Item3), hoveredTileData.Item3);
                }
            }

            //if right mouse clicked un pick the tile.
            if (InputManager.Instance.IsRightMouseToggled())
            {
                //un-pick tile (set false flag)
                pickedUpTileData = new Tuple<Vector2, bool, bool>(Vector2.Zero, false, false);
            }
        }

        private Unit GetUnitFromTileData(Tuple<Vector2, bool, bool> data)
        {
            //get unit from bench or grid
            return data.Item3? turnPlayer.GetBenchUnits()[(int)data.Item1.X] : grid.GetUnitFromCoordinate(CoordConverter.OffsetToCube(data.Item1));
        }

        private bool DoesUnitExistAtDataPoint(Tuple<Vector2, bool, bool> data)
        {
            return GetUnitFromTileData(data) != null;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //allows pixel-perfect upscaling of sprites using point clamp spriteBatch
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            graphicsDevice.Clear(Color.Orange);
            Draw_Grid(spriteBatch);
            Draw_Bench(spriteBatch);
            Draw_Units(spriteBatch);
            Draw_TopLeftUI(spriteBatch);
            turnPlayer.Shop.Draw(spriteBatch, unitSprites, moneyIcon, bebasSmall);
            spriteBatch.End();
        }

        private void Draw_Grid(SpriteBatch spriteBatch)
        {

            for (int y = (int)grid.Size.Y - 1; y > -1; y--)
            {
                for (int x = (int)grid.Size.X - 1; x > -1; x--)
                {
                    //draw a gray tile if the tile in question is in the opponents half
                    if (pickedUpTileData.Item2 && (!turnPlayer.IsPointInHalf(grid.Tiles[x, y].OffsetCoordinate, false)))
                    {
                        spriteBatch.Draw(isoTile, position: grid.Tiles[x, y].PixelCoordinate, scale: boardScaleVector, color: Color.Gray);
                    }
                    //draw a red tile if there is a picked up tile from the bench and the 
                    //  unit cap has been reached, and there is no unit on the hovered tile.
                    //  (unit cannot be placed on hovered tile).
                    else if (pickedUpTileData.Item2 && pickedUpTileData.Item3 && grid.CountPlayerUnitsOnGrid(turnPlayer.Id) >= turnPlayer.UnitCap &&
                            ((grid.GetUnitFromCoordinate(CoordConverter.OffsetToCube(new Vector2(x, y))) == null) ?
                            true : !(grid.GetUnitFromCoordinate(CoordConverter.OffsetToCube(new Vector2(x, y))).OwnerId == turnPlayer.Id)))
                    {
                        spriteBatch.Draw(isoTile, position: grid.Tiles[x, y].PixelCoordinate, scale: boardScaleVector, color: Color.DarkRed);

                    }
                    //otherwise draw an outline around the tile if its hovered.
                    else if (hoveredTileData.Item2 && !hoveredTileData.Item3 && grid.Tiles[x, y].OffsetCoordinate == hoveredTileData.Item1)
                    {
                        //note the position must be decremented by (1,1)*boardScale as the sprite is (1,1) larger.
                        spriteBatch.Draw(hoveredIsoTile, position: grid.Tiles[x, y].PixelCoordinate - boardScaleVector, scale: boardScaleVector);
                    }
                    else
                    {
                        spriteBatch.Draw(isoTile, position: grid.Tiles[x, y].PixelCoordinate, scale: boardScaleVector);
                    }
                }
            }
        }

        private void Draw_Bench(SpriteBatch spriteBatch)
        {
            for (int y = (int)bench.Size.Y - 1; y > -1; y--)
            {
                for (int x = (int)bench.Size.X - 1; x > -1; x--)
                {
                    //draw hovered tile if hovered...
                    if (hoveredTileData.Item2 && hoveredTileData.Item3 && bench.Tiles[x, y].OffsetCoordinate == hoveredTileData.Item1)
                    {
                        //note the position must be decremented by (1,1)*boardScale as the sprite is (1,1) larger.
                        spriteBatch.Draw(hoveredIsoTile, position: bench.Tiles[x, y].PixelCoordinate - boardScaleVector, scale: boardScaleVector);
                    }
                    else
                    {
                        spriteBatch.Draw(isoTile, position: bench.Tiles[x, y].PixelCoordinate, scale: boardScaleVector);

                    }
                }
            }
        }

        private void Draw_AugmentIcon(SpriteBatch spriteBatch, Unit unit, Vector2 unitPos, Texture2D unitTexture)
        {

            //draw an icon to show that the stats of the unit have been
            // augmented according to the player's chosen character's ability.
            if (unit.Augmented)
            {
                spriteBatch.Draw(augmentIcon,
                    position: unitPos + boardScaleVector / 2 * UiTools.BoundsToVector(unitTexture.Bounds) - uiScale / 2 * UiTools.BoundsToVector(augmentIcon.Bounds),
                    scale: uiScaleVector
                    );
            }
        }

        private void Draw_Units(SpriteBatch spriteBatch)
        {
            //draw bench units
            for (int x = (int)bench.Size.X-1; x >= 0; x--)
            {
                //get unit on current bench tile iter
                Unit unit = turnPlayer.GetBenchUnits()[x];

                if (unit != null)
                {
                    //draw if not picked up
                    if (GetUnitFromTileData(pickedUpTileData) != unit || !pickedUpTileData.Item2)
                    {
                        Texture2D unitTexture = unit.GetSprite(unitSprites);
                        Vector2 pos = bench.Tiles[x, 0].GetCentrePixelCoordinate(boardScale) - new Vector2(boardScale * (unitTexture.Width / 2), unitTexture.Height * boardScale);
                        spriteBatch.Draw(unitTexture, position: pos, scale: boardScaleVector);
                        Draw_AugmentIcon(spriteBatch, unit, pos, unitTexture);
                    }
                }
            }

            //draw units on the board - must be done from back of the grid to front
            // to preserve perspective.
            foreach (Unit unit in grid.Units.OrderByDescending(i => i.CubeCoordinate.Z).ThenBy(i => i.CubeCoordinate.Y))
            {
                Vector2 tileCoordinate = CoordConverter.CubeToOffset(unit.CubeCoordinate);
                if (turnPlayer.IsPointInHalf(tileCoordinate, false))
                {
                    if (GetUnitFromTileData(pickedUpTileData) != unit || !pickedUpTileData.Item2)
                    {
                        Texture2D unitTexture = unit.GetSprite(unitSprites);
                        Vector2 pos = grid.Tiles[(int)tileCoordinate.X, (int)tileCoordinate.Y].GetCentrePixelCoordinate(boardScale) - new Vector2(boardScale * (unitTexture.Width / 2), unitTexture.Height * boardScale);
                        spriteBatch.Draw(unitTexture, position: pos, scale: boardScaleVector);
                        Draw_AugmentIcon(spriteBatch, unit, pos, unitTexture);
                    }
                }
            }

            //draw the picked up unit hovering over the mouse position
            if (pickedUpTileData.Item2)
            {
                Unit toDraw = GetUnitFromTileData(pickedUpTileData);
                Texture2D texture = toDraw.GetSprite(unitSprites);
                Vector2 pos = InputManager.Instance.MousePos - boardScale / 2 * UiTools.BoundsToVector(texture.Bounds);

                spriteBatch.Draw(texture,
                    position: pos,
                    scale: boardScaleVector
                    );
                Draw_AugmentIcon(spriteBatch, toDraw, pos, texture);
            }
        }

        private void Draw_TopLeftUI(SpriteBatch spriteBatch)
        {
            //Draw the win progress bar
            Vector2 barPos = characterDrawPos + new Vector2(uiScale * turnPlayer.CharacterSprite.Width / 2, 0f);
            PercentageBar.Draw(spriteBatch, emptyRect, barPos, winBarDimensions, turnPlayer.Colour, Color.Gray, turnPlayer.WinThreshold, turnPlayer.WinProgress);
            string text = $"{turnPlayer.WinProgress}/{turnPlayer.WinThreshold} Wins";
            spriteBatch.DrawString(bebasSmall, text, barPos + (winBarDimensions - 0.6f * bebasSmall.MeasureString(text)) / 2, Color.White, 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);

            //Draw the unit cap bar.
            Vector2 unitBarPos = barPos + new Vector2(0, winBarDimensions.Y);
            int unitCount = grid.CountPlayerUnitsOnGrid(turnPlayer.Id);
            PercentageBar.Draw(spriteBatch, emptyRect, unitBarPos, unitBarDimensions, Color.DarkRed, Color.YellowGreen, turnPlayer.UnitCap, unitCount);
            text = $"{unitCount}/{turnPlayer.UnitCap} Units";
            spriteBatch.DrawString(bebasSmall, text, unitBarPos + (unitBarDimensions - 0.5f * bebasSmall.MeasureString(text)) / 2, Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);

            //Draw the players money amount.
            moneyBox.Draw(spriteBatch);
            string moneyText = $"{turnPlayer.Money}";
            Vector2 textSize = bebasSmall.MeasureString(moneyText);
            Vector2 basePos = moneyBox.GetPos() + moneyBoxDimensions/2;
            spriteBatch.DrawString(bebasSmall, moneyText, basePos - new Vector2(0, textSize.Y/2), Color.White);
            spriteBatch.Draw(moneyIcon,
                position: basePos + new Vector2(textSize.X + moneyBoxDimensions.X/35, 0) - uiScale/2 * new Vector2(0, moneyIcon.Height),
                scale: uiScaleVector
                );

            //Draw the player's character.
            spriteBatch.Draw(texture: characterBoundary, position: characterDrawPos - new Vector2(3 * uiScale, 0), scale: uiScaleVector, color: turnPlayer.Colour);
            spriteBatch.Draw(texture: turnPlayer.CharacterSprite, position: characterDrawPos, scale: uiScaleVector);
            
            //draw end turn button
            if (endTurnButton.IsHovered)
            {
                spriteBatch.Draw(texture: endTurnButton.HoverTexture, position: endTurnButton.Position, scale: uiScaleVector);
            }
            else
            {
                spriteBatch.Draw(texture: endTurnButton.ButtonTexture, position: endTurnButton.Position, scale: uiScaleVector);
            }
            spriteBatch.DrawString(bebasSmall, "End Turn", endTurnButton.MidPoint - 0.6f * bebasSmall.MeasureString("End Turn") / 2 + new Vector2(0, 1), Color.White, 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0);

            //draw the level up unit cap button.
            levelUnitCapButton.Draw(spriteBatch, turnPlayer, moneyIcon, WindowTools.GetUpscaledValue(2f), bebasSmall, 0.6f);

            //and the ability button
            useAbilityButton.Draw(spriteBatch, turnPlayer, bebasSmall, 0.6f);

            divisorLine.Draw(spriteBatch);

            stageInfo.Draw(spriteBatch, vsAiIcon, vsPlayerIcon);

            //help button
            helpButton.Draw(spriteBatch, bebasSmall, 1f);
        }
    }
}