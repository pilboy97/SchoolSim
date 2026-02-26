using System;

namespace Game.Object.Character
{
    [Serializable]
    public struct CharacterRelation
    {
        public enum Type
        {
            Friend,
            Romance
        }

        public Type relType;
        public string ID;
    }
}