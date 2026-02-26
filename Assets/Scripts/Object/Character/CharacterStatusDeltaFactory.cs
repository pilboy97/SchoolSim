using System;
using System.Collections.Generic;

namespace Game.Object.Character
{
    
    [Serializable]
    public class CharacterStatusDeltaFactory : IEquatable<CharacterStatusDeltaFactory>, ICloneable
    {
        public StatusFloatDict dict;
        public const string MEMBER = "[MEMBER]";
        
        public CharacterStatusDeltaFactory()
        {
            dict = new StatusFloatDict();
        }
        
        public CharacterStatusDeltaFactory(Dictionary<CharacterStatus, float> val) : this()
        {
            foreach (var kv in val)
            {
                dict[kv.Key] = kv.Value;
            }
        }

        public CharacterStatusDeltaFactory Add(CharacterStatus statusType, float value)
        {
            dict.TryAdd(statusType, 0);
            dict[statusType] += value;

            return this;
        }
        public CharacterStatusDeltaFactory Add(CharacterStatusDeltaFactory deltas)
        {
            foreach (var (s,v) in deltas.dict)
            {
                Add(s, v);
            }
            
            return this;
        }
        
        public void Apply(Character subject, bool perSec = true, List<Character> members = null)
        {
            foreach (var (key, value) in dict)
            {
                subject.Apply(key, value * (perSec ? UnityEngine.Time.deltaTime : 1));
            }
        }

        public float this[CharacterStatus s] => dict?.GetValueOrDefault(s, 0) ?? 0;


        public static CharacterStatusDeltaFactory operator * (CharacterStatusDeltaFactory x, float y)
        {
            var ret = new CharacterStatusDeltaFactory();

            foreach (var kv in x?.dict ?? new())
            {
                ret.dict.TryAdd(kv.Key, 0);
                ret.dict[kv.Key] += kv.Value * y;
            }

            return ret;
        }
        public static CharacterStatusDeltaFactory operator + (CharacterStatusDeltaFactory x, CharacterStatusDeltaFactory y)
        {
            var ret = new CharacterStatusDeltaFactory();

            foreach (var kv in x?.dict ?? new())
            {
                ret.dict.TryAdd(kv.Key, 0);
                ret.dict[kv.Key] += kv.Value;
            }
            foreach (var kv in y?.dict ?? new())
            {
                ret.dict.TryAdd(kv.Key, 0);
                ret.dict[kv.Key] += kv.Value;
            }

            return ret;
        }

        public bool Equals(CharacterStatusDeltaFactory other)
        {
            if (other is null) return false;
            return ReferenceEquals(this, other) || Equals(dict, other.dict);
        }
        
        public CharacterStatusDeltaFactory DeltaStats(Character sub)
        {
            var result = new CharacterStatusDeltaFactory();
            
            foreach (var (key, value) in dict)
            {
                var r = sub.CalcPersonalizedStatsDeltaOnReceiveStatsDelta(key, value);
                foreach (var (s,v) in r.dict) 
                {
                    result.Add(s,v);
                }
            }
            return result;
        }

        public object Clone()
        {
            var ret = new CharacterStatusDeltaFactory();

            foreach (var (key, value) in dict)
            {
                ret.Add(key, value);
            }

            return ret;
        }
    }
}