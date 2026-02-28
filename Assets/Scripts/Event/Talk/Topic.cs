using System;
using Game.Object.Character;

namespace Game.Event.Talk
{
    [Serializable]
    public struct Topic
    {
        public enum Type
        {
            General,
            Romance,
            Teach,
            RelationUp,
            RelationDown
        }

        public Type type;
        public Character speaker;
        public CharacterStatsType knowledge;
        
        public Character target;

        public override string ToString()
        {
            return type switch
            {
                Type.General or Type.Romance  => Enum.GetName(typeof(Type), type),
                Type.Teach => $"Teach {knowledge}",
                Type.RelationUp => $"Compliment {target.charName}",
                Type.RelationDown => $"Put down {target.charName}",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}