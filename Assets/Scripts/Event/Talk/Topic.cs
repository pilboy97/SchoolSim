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
            Subject, 
            Skill,
            RelationUp,
            RelationDown
        }

        public Type type;
        public Character subject;
        public CharacterStatus topic;
        public Character target;

        public override string ToString()
        {
            return type switch
            {
                Type.General or Type.Romance  => Enum.GetName(typeof(Type), type),
                Type.Subject or Type.Skill => Enum.GetName(typeof(CharacterStatus), topic),
                Type.RelationUp => $"Compliment {target.Name}",
                Type.RelationDown => $"Put down {target.Name}",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}