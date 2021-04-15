using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Engine.UI
{
    public static class WindowTools
    {
        //16:9 aspect ratios
        public static Vector2 p720 = new Vector2(1280, 720);
        public static Vector2 p1080 = new Vector2(1920, 1080);
        public static Vector2 p1440 = new Vector2(2560, 1440);

        //resolution window is currently at
        public static Vector2 WindowDimensions = p1080;
        public static Dictionary<Vector2, float> upscaleRatios = new Dictionary<Vector2, float>
        {
            { p720, 1f },
            { p1080, 1.5f },
            { p1440, 2f }
        };

        public static float GetUpscaleRatio()
        {
            return upscaleRatios[WindowDimensions];
        }
            
        public static float GetUpscaledValue(float baseScale)
        {
            return GetUpscaleRatio() * baseScale;
        }

        /// <summary>
        /// Converts a coordinate which is a fraction of the window bounds and incorporates
        /// pixel padding to the actual screen-space coordinate. (I.e. (0f, 0f, 10, 10) -> (10, 10) )
        /// </summary>
        public static Vector2 PaddingToPixelCoordinate(float fractionX, float fractionY, int paddingX, int paddingY)
        {
            //note inline ifs here are for if a -ve fraction is given: calculation must be relative
            //to the right side/bottom of the screen opposed to left side/top of the screen.
            float x = (fractionX >= 0) ? WindowDimensions.X * fractionX + paddingX : WindowDimensions.X + (WindowDimensions.X * fractionX) - paddingX;
            float y = (fractionY >= 0) ? WindowDimensions.Y * fractionY + paddingY : WindowDimensions.Y + (WindowDimensions.Y * fractionY) - paddingY;

            return new Vector2(x, y);
        }
    }
}
