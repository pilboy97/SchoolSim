using System;
using Game.Object;
using Game.Object.Character;
using UnityEngine;

namespace Game.Task
{
    [Serializable]
    public struct Action : IEquatable<Action>
    {
        [SerializeField] public string actionName;
        [SerializeReference] public Effect effect;
        [SerializeReference] public Condition cond;
        [SerializeField] public bool indirect;
        [SerializeField] public bool busy;
        [SerializeField] public bool notOnce;
        [SerializeField] public bool allowSelf;

        public (CharacterStats, RelationFloatDict) DeltaStats(Character s, IInteractable o)
        {
            return effect?.DeltaStats(s, o) ?? default;
        }

        public bool Equals(Action other)
        {
            return actionName == other.actionName && Equals(effect, other.effect);
        }

        public override bool Equals(object obj)
        {
            return obj is Action other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(actionName, effect);
        }
        
        public bool Check(Character subject, IInteractable other)
        {
            if (!allowSelf && subject.ID == other.ID) return false;
            
            return cond?.Check(null, this,subject ,other) ?? true;
        }
    }
}