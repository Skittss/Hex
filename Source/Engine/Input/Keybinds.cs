using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Engine.Input
{
    public static class Keybinds
    {
        public enum Actions
        {
            Continue,
            Exit,
            Refresh,
            SellUnit
        }

        //hold a dictionary of actions to keys binded to that action.
        // makes it much easier to maintain code if changing around bindings
        private static Dictionary<Actions, Keys> bindings = new Dictionary<Actions, Keys>
        {
            {Actions.Continue, Keys.Space},
            {Actions.Exit, Keys.Escape},
            {Actions.Refresh, Keys.R},
            {Actions.SellUnit, Keys.LeftShift}
        };

        public static Keys GetKey(Actions action)
        {
            //return keys
            return bindings[action];
        }

    }
}
