using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Engine.UI;
using Engine.GameStates;
using Hex.UI;

namespace Hex.GameStates
{
    public abstract class PassTurnScreen : GameState
    {
        protected static float uiScale = WindowTools.GetUpscaledValue(6f);
        protected Vector2 uiScaleVector = new Vector2(uiScale);
        protected SinglePressSpriteButton passTurnButton;

        protected SpriteFont bebasSmall;
        protected SpriteFont bebas;

        protected Texture2D boundary;
        protected Texture2D vsAiIcon;
        protected Texture2D vsPlayerIcon;

        public PassTurnScreen(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {

        }

        public override void Initialize()
        {
            
        }

        public override void LoadContent()
        {
            LoadContent_Button(contentManager);
            LoadContent_Icons(contentManager);
            boundary = contentManager.Load<Texture2D>("CharacterSelect/Buttons/ButtonBoundaryLR");
            bebasSmall = contentManager.Load<SpriteFont>("Fonts/BebasNeue_Small");
            bebas = contentManager.Load<SpriteFont>("Fonts/BebasNeue");
        }

        private void LoadContent_Icons(ContentManager Content)
        {
            vsAiIcon = Content.Load<Texture2D>("PreparationTurn/vsPlayerIcon");
            vsPlayerIcon = Content.Load<Texture2D>("PreparationTurn/vsPlayerIcon");
        }

        private void LoadContent_Button(ContentManager Content)
        {
            //generate the pass turn button and assign a method to it.
            Texture2D passTurnButtonTexture = Content.Load<Texture2D>("PreparationTurn/EndTurnButton");
            Texture2D hoverTexture = Content.Load<Texture2D>("PreparationTurn/EndTurnButtonHovered");
            passTurnButton = new SinglePressSpriteButton(WindowTools.PaddingToPixelCoordinate(0.5f, 0.6f, 0, 0) - (0.5f * uiScale * UiTools.BoundsToVector(passTurnButtonTexture.Bounds)), passTurnButtonTexture, hoverTexture, UiTools.CreateBoolMask(passTurnButtonTexture), uiScale);
            passTurnButton.buttonPressed += HandlePassTurnButtonPress;
        }
        private void HandlePassTurnButtonPress(object sender, SinglePressEventArgs eventArgs)
        {
            GameStateManager.Instance.PopState();
        }

        public override void Update(GameTime gameTime)
        {
            passTurnButton.Update(gameTime);
        }
    }
}
