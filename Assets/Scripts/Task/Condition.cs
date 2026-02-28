using System;
using System.Collections.Generic;
using Game.Object;
using Game.Object.Character;
using Game.Time;
using UnityEngine;

namespace Game.Task
{
    interface ICondition
    {
        bool Check(Event.Event e, Action a, Character sub, IInteractable obj);
    }
    [Serializable]
    public abstract class Condition : ICondition
    {
        public virtual bool Check(Event.Event e, Action a, Character sub, IInteractable obj)
        {
            return true;
        }
    }

    [Serializable]
    public class AlwaysTrueCond : Condition
    {
        
    }

    [Serializable]
    public class AlwaysFalseCond : Condition
    {
        public override bool Check(Event.Event e, Action a, Character sub, IInteractable obj)
        {
            return false;
        }
    }
    
    [Serializable]
    public class BlackListCond : Condition
    {
        [SerializeField] public List<Character> blacklist = new();

        public override bool Check(Event.Event e, Action a, Character sub, IInteractable obj)
        {
            return !blacklist.Contains(sub);
        }
    }
    
    [Serializable]
    public class WhiteListCond : Condition
    {
        [SerializeField] public List<Character> whitelist = new();

        public override bool Check(Event.Event e, Action a, Character sub, IInteractable obj)
        {
            return whitelist.Contains(sub);
        }
    }

    [Serializable]
    public class EventMemberNumberCond : Condition
    {
        [SerializeField] public int minMember = 2;
        [SerializeField] public int maxMember = 8;
        
        public override bool Check(Event.Event e, Action a, Character sub, IInteractable obj)
        {
            if (minMember == -1 && maxMember == -1) return true;
            if (minMember == -1) return e.members.Count <= maxMember;
            if (maxMember == -1) return e.members.Count >= minMember;

            return minMember <= e.members.Count && e.members.Count <= maxMember;
        }
    }

    [Serializable]
    public class StatusCond : Condition
    {
        public enum Type
        {
            Larger,
            LargeOrEqual,
            Equal,
            Lesser,
            LessOrEqual,
            NotEqual
        }

        [SerializeField] public CharacterStatsType stats;
        [SerializeField] public Type cmpType;
        [SerializeField] public float value;
        
        public override bool Check(Event.Event e, Action a, Character sub, IInteractable obj)
        {
            var subStat = sub.Data[stats];
            
            return cmpType switch
            {
                Type.Larger => subStat > value,
                Type.LargeOrEqual => subStat >= value,
                Type.Equal => Mathf.Approximately(subStat, value),
                Type.Lesser => subStat < value,
                Type.LessOrEqual => subStat <= value,
                Type.NotEqual => !Mathf.Approximately(subStat, value),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    [Serializable]
    public class AndCond : Condition
    {
        [SerializeReference] public List<Condition> conds = new();
        
        public override bool Check(Event.Event e, Action a, Character sub, IInteractable obj)
        {
            foreach (var cond in conds)
            {
                if (cond == null) continue;
                if (!cond.Check(e, a, sub, obj))
                {
                    return false;
                }
            }

            return true;
        }
    }

    [Serializable]
    public class OrCond : Condition
    {
        [SerializeReference] public List<Condition> conds = new();
        
        public override bool Check(Event.Event e, Action a, Character sub, IInteractable obj)
        {
            foreach (var cond in conds)
            {
                if (cond == null) continue;
                if (cond.Check(e, a, sub, obj))
                {
                    return true;
                }
            }

            return false;
        }
    }
    
    
    [Serializable]
    public class AllMemberCond : Condition
    {
        [SerializeReference] public Condition cond;
        
        public override bool Check(Event.Event e, Action a, Character sub, IInteractable obj)
        {
            foreach (var member in e.members)
            {
                if (!cond.Check(e, a, member, null))
                {
                    return false;
                }
            }

            return true;
        }
    }
    
    [Serializable]
    public class AnyMemberCond : Condition
    {
        [SerializeReference] public Condition cond;
        
        public override bool Check(Event.Event e, Action a, Character sub, IInteractable obj)
        {
            foreach (var member in e.members)
            {
                if (cond.Check(e, a, member, null))
                {
                    return true;
                }
            }

            return false;
        }
    }

    [Serializable]
    public class NotCond : Condition
    {
        [SerializeReference] public Condition cond;
        
        public override bool Check(Event.Event e, Action a, Character sub, IInteractable obj)
        {
            return cond.Check(e, a, sub, obj);
        }
    }

    [Serializable]
    public class IsCloseEnoughCond : Condition
    {
        [SerializeField] public float dist;
        
        public override bool Check(Event.Event e, Action a, Character sub, IInteractable obj)
        {
            return obj.Distance(sub) < dist;
        }
    }

    [Serializable]
    public class TimeRangeCond : Condition
    {
        [SerializeField] public ulong begin = 0;
        [SerializeField] public ulong end = int.MaxValue;

        public override bool Check(Event.Event e, Action a, Character sub, IInteractable obj)
        {
            var tick = TimeManager.Instance.Ticks;

            return begin <= tick && tick < end;
        }
    }

    [Serializable]
    public class DailyTimeRangeCond : TimeRangeCond
    {
        public override bool Check(Event.Event e, Action a, Character sub, IInteractable obj)
        {
            begin %= 24 * 60 * 60;
            end %= 24 * 60 * 60;
            
            var tick = TimeManager.Instance.Ticks % (24 * 60 * 60);

            return begin <= tick && tick < end;
        }
    }

    [Serializable]
    public class DayFlagCond : Condition
    {
        [SerializeField] public DayFlag flag;   
        public override bool Check(Event.Event e, Action a, Character sub, IInteractable obj)
        {
            var (_, _, _, _, f) = Calendar.ToDate();

            return flag.Check(f);
        }
    }
}