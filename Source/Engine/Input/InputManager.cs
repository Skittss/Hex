using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Engine.Input
{
    public class InputManager
    {
        private KeyboardState currentKeyboardState;
        private KeyboardState previousKeyboardState;

        private MouseState currentMouseState;
        private MouseState previousMouseState;

        public Vector2 MousePos { get { return new Vector2(currentMouseState.Position.X, currentMouseState.Position.Y); } }

        //Like GameStateManager - a singleton
        private static InputManager instance;
        public static InputManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new InputManager();
                }
                return instance;
            }
        }

        public InputManager()
        {
            //Set the first two states on instantiation so that they are not
            // read null on first time read.
            currentKeyboardState = Keyboard.GetState();
            previousKeyboardState = Keyboard.GetState();

            currentMouseState = Mouse.GetState();
            previousMouseState = Mouse.GetState();
        }

        public void Update()
        {
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();
        }

        public bool IsKeyPressed(Keybinds.Actions action)
        {
            if (currentKeyboardState.IsKeyDown(Keybinds.GetKey(action)))
            {
                return true;
            }
            return false;
        }

        public bool IsKeyToggled(Keybinds.Actions action)
        {
            //toggle from up to down
            if (currentKeyboardState.IsKeyDown(Keybinds.GetKey(action)) && previousKeyboardState.IsKeyUp(Keybinds.GetKey(action)))
            {
                return true;
            }
            return false;
        }

        public Keys[] GetCurrentPressed()
        {
            return currentKeyboardState.GetPressedKeys();
        }

        public bool IsLeftMouseToggled()
        {
            return (currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released);
        }

        public bool IsRightMouseToggled()
        {
            return (currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released);
        }
    }
}
