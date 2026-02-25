using System;
using System.Collections.Generic;

namespace Game.Object.Character
{
    [Serializable]
    public enum CharacterStatus
    {
        Begin,
        IntBegin,
        Logic,
        Language,
        Aesthetics,
        Social,
        Athletic,
        Craft,
        IntEnd,
        SkillBegin,
        Speech,
        Writing,
        Comedy,
        Painting,
        Music,
        Dance,
        Swim,
        Soccer,
        Conversation,
        Attractive,
        Cook,
        Combat,
        Run,
        Gym,
        Coding,
        SkillEnd,
        SubjectBegin,
        Literature,
        Math,
        Sociology,
        Science,
        Sports,
        Art,
        SubjectEnd,
        NeedsBegin,
        ENeedsBegin,
        Hungry,
        Fatigue,
        Toilet,
        Hygiene,
        ENeedsEnd,
        RNeedsBegin,
        Loneliness,
        RLoneliness,
        Fun,
        RNeedsEnd,
        NeedsEnd,
        GNeedsBegin,
        Motivation,
        GNeedsEnd,
        End
    }

    public static class CharacterStatusHelper
    {
        private static Dictionary<string, CharacterStatus> _cache;
        
        public static CharacterStatus FromString(string id)
        {
            if (_cache.TryGetValue(id, out var value)) return value;

            var ret = (CharacterStatus)Enum.Parse(typeof(CharacterStatus), id);
            
            _cache.Add(id, ret);

            return ret;
        }
    }
}