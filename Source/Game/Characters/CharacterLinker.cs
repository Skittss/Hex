using System.Collections.Generic;
using System;

namespace Hex.Characters
{
    /// <summary>
    /// Links the class type of characters to a respective id.
    /// Allows a character class to be referenced using a linking integer,
    /// then invoking the class constructor using the reference string.
    /// Used mainly to load character sprites and ability descriptions.
    /// </summary>
    public static class CharacterLinker
    {
        //use a dictionary for easy lookup.
        private static Dictionary<int, string> linkDict = new Dictionary<int, string>
        {
            {0, "Hex.Characters.Knight"},
            {1, "Hex.Characters.Sorcerer"},
            {2, "Hex.Characters.Hunter"},
            {3, "Hex.Characters.Bard"},
            {4, "Hex.Characters.Scout"}
        };

        public static Type GetCharacterReference(int linkId)
        {
            return Type.GetType(linkDict[linkId]);
        }

        public static int GetCharacterCount()
        {
            return linkDict.Count;
        }
    }
}
