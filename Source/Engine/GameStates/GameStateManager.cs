using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;


/// <summary>
/// GameStateManager is a singleton, i.e. always one instance only at runtime
/// </summary>
namespace Engine.GameStates
{
    public class GameStateManager
    {
        //This class simply manages a stack of running GameStates.
        //could be considered bad practice to use a singleton, however in 
        //this case it is more of a way to control the class, and not be left without
        //a StateManager if needed.

        private Stack<GameState> states = new Stack<GameState>();
        
        private static GameStateManager instance;
        public static GameStateManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameStateManager();
                }
                return instance;
            }
        }

        //storing the content manager so that gamestates
        // can create their own content manager
        // by referencing content.ServiceProvider.
        public ContentManager Content;

        public void SetContent(ContentManager content)
        {
            //since this class is a singleton, it can have an instance without being parsed anything
            //therefore the contentmanager (and any other required env vars) must be set
            //through a seperate function

            Content = content;
        }

        public void Update(GameTime gameTime)
        {
            states.Peek().Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            states.Peek().Draw(spriteBatch);
        }

        public void AddState(GameState state)
        {
            states.Push(state);
            states.Peek().Initialize();
            states.Peek().LoadContent();
        }

        public void PopState()
        {
            states.Peek().UnloadContent();
            states.Pop();
        }
            
        public void PopAllStates()
        {
            while (states.Count > 0)
            {
                PopState();
            }
        }

        public void ChangeState(GameState state)
        {
            PopAllStates();
            AddState(state);
        }

        public void UnloadAllStateContent()
        {
            foreach (GameState state in states)
            {
                state.UnloadContent();
            }
        }
    }
}
