using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Engine.UI;
using Engine.GameStates;
using System.Collections.Generic;
using System;
using Hex.BoardGame;
using Hex.Characters;

namespace Hex.GameStates
{
    public class BoardGame: GameState
    {
        //class to keep track of the current turn.
        private class TurnManager
        {
            private static int queueLoadNumber = 3;

            private int stageNumber = 0;
            private int stageIndex = 0;
            private int subTurnNumber = 0;
            public int StageNumber { get { return stageNumber; } }
            public int StageIndex { get { return stageIndex; } }
            //whether it is prep turn 1, prep turn 2 or battle turn (0, 1, 2 respectively)
            public int SubTurn { get { return subTurnNumber; } }

            private Queue<Stage> stages = new Queue<Stage>();

            public TurnManager() 
            { 
                //generate some stages to begin with.
                for (int i = 1; i < queueLoadNumber + 1; i++)
                {
                    stages.Enqueue(new Stage(i));
                }
            }

            private void NextStage()
            {
                Stage stage = stages.Dequeue();
                stages.Enqueue(new Stage(stage.StageNumber + queueLoadNumber)); 
                //keep the queue loaded with {queueLoadNumber (3)} stages at all times
            }

            public void Increment()
            {
                if (stageIndex == GetCurrentStage().Length - 1 && subTurnNumber == 2)
                {
                    stageIndex = 0;
                    subTurnNumber = 0;
                    stageNumber += 1;
                    //advance to the next stage if the current one finishes.
                    NextStage();
                }
                else
                {
                    //mod the subturn number with 3 as only 3 possibilities (0, 1, 2)
                    subTurnNumber = (subTurnNumber + 1) % 3;
                    if (subTurnNumber == 0)
                    {
                        stageIndex += 1;
                    }
                }
            }

            public Stage GetCurrentStage()
            {
                return stages.Peek();
            }

            public bool IsCurrentRoundVsAi()
            {
                return stages.Peek().IsRoundVsAi(stageIndex);
            }

        }

        private TurnManager turnCounter = new TurnManager();

        //information specific to this game
        // (classes chosen etc.)
        public struct GameArgs
        {
            public readonly int p1LinkId;
            public readonly int p2LinkId;
            public readonly Color p1Color;
            public readonly Color p2Color;
            public readonly int winThreshold;
            public GameArgs(int p1LinkId, int p2LinkId, Color p1Color, Color p2Color, int winThreshold)
            {
                this.p1LinkId = p1LinkId;
                this.p2LinkId = p2LinkId;
                this.p1Color = p1Color;
                this.p2Color = p2Color;
                this.winThreshold = winThreshold;
            }
        }

        private GameArgs gameArgs;

        //Players
        private Player[] players = new Player[2];

        //Grid Information
        private HexGrid grid;
        private static int playerHalfSize = 3;  //rows
        private static Vector2 gridSize = new Vector2(7f, 2*playerHalfSize);
        private static Vector2 screenBoardOrigin = WindowTools.PaddingToPixelCoordinate(0.33f, 0.70f, 0, 0);
        private static float boardScale = WindowTools.GetUpscaledValue(5f);

        public BoardGame(GraphicsDevice graphicsDevice, GameArgs gameArgs) : base(graphicsDevice)  //end bit here calls the base constructor.
        {
            this.gameArgs = gameArgs;
        }

        public override void Initialize()
        {
            //reset the id leaser upon starting a new game.
            UnitIdLeaser.Instance.Reset();
        }

        public override void LoadContent()
        {
            LoadContent_Players(contentManager);
            GenerateBoard();
        }

        private void LoadContent_Players(ContentManager Content)
        {
            //get the sprites textures for both players characters (to display in top left)
            Type[] characterReferences = new Type[]
            {
                CharacterLinker.GetCharacterReference(gameArgs.p1LinkId),
                CharacterLinker.GetCharacterReference(gameArgs.p2LinkId)
            };
            string[] spriteDirectories = new string[]
            {
                (string)characterReferences[0].GetField("SpriteReference").GetRawConstantValue(),
                (string)characterReferences[1].GetField("SpriteReference").GetRawConstantValue()
            };
            Texture2D[] characterSprites = new Texture2D[]
            {
                Content.Load<Texture2D>(spriteDirectories[0]),
                Content.Load<Texture2D>(spriteDirectories[1])
            };


            players[0] = new Player(0, characterReferences[0], graphicsDevice, characterSprites[0], gameArgs.p1Color, gameArgs.winThreshold);
            players[1] = new Player(1, characterReferences[1], graphicsDevice, characterSprites[1], gameArgs.p2Color, gameArgs.winThreshold);
        }

        private void GenerateBoard()
        {
            grid = new HexGrid((int)gridSize.X, (int)gridSize.Y, screenBoardOrigin, boardScale);
            grid.AssignHalves(players[0], players[1]);        
        }

        public override void Update(GameTime gameTime)
        {
            int subTurnType = turnCounter.SubTurn;

            //add a battle phase if the subturn is 2 (0 = p1 prep turn, 1 = p2 prep turn, 2 = battle)
            if (turnCounter.SubTurn == 2)
            {
                if (turnCounter.IsCurrentRoundVsAi())
                {
                    Unit[] aiRoundUnits = turnCounter.GetCurrentStage().GenerateAiUnits();
                    GameStateManager.Instance.AddState(new BattleTurn(graphicsDevice, grid, players, turnCounter.StageNumber, true, players[1].Id, aiRoundUnits));
                    GameStateManager.Instance.AddState(new PassTurnToBattleTurn(graphicsDevice, new Player[] { null, players[1] }, true));
                    GameStateManager.Instance.AddState(new BattleTurn(graphicsDevice, grid, players, turnCounter.StageNumber, true, players[0].Id, aiRoundUnits));
                    GameStateManager.Instance.AddState(new PassTurnToBattleTurn(graphicsDevice, new Player[] { players[0], null }, true));
                }
                else
                {
                    GameStateManager.Instance.AddState(new BattleTurn(graphicsDevice, grid, players, turnCounter.StageNumber, false, -1 ,null));
                    GameStateManager.Instance.AddState(new PassTurnToBattleTurn(graphicsDevice, players, false));

                }
            }
            else
            {
                GameStateManager.Instance.AddState(new PreparationTurn(graphicsDevice, players[subTurnType], grid, turnCounter.StageNumber, turnCounter.StageIndex, turnCounter.GetCurrentStage()));
                GameStateManager.Instance.AddState(new PassTurnToPrepTurn(graphicsDevice, players[subTurnType]));
            }
            //add the turn entry screen followed by the actual screen, so entry screen is updated first
            //  until popped from the stack. 
            //  (players are given an opportunity to take turns viewing the screen)

            turnCounter.Increment();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

        }

    }

}