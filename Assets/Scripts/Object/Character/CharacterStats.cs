using System;

namespace Game.Object.Character
{
    [Serializable]
    public struct CharacterStats : IEquatable<CharacterStats>
    {
        public float logic;
        public float language;
        public float aesthetics;
        public float social;
        public float athletic;
        /*-------------------------------*/
        public float comedy;
        public float conversation;
        public float attractive;
        /*-------------------------------*/
        public float literature;
        public float math;
        public float sociology;
        public float science;
        public float sports;
        public float art;
        /*-------------------------------*/
        public float hungry;
        public float fatigue;
        public float toilet;
        public float hygiene;
        public float loneliness;
        public float rLoneliness;
        public float fun;
        public float motivation;

        
        #region 덧셈 (+)
        public static CharacterStats operator +(CharacterStats x, CharacterStats y)
        {
            return new CharacterStats()
            {
                logic = x.logic + y.logic, language = x.language + y.language, aesthetics = x.aesthetics + y.aesthetics,
                social = x.social + y.social, athletic = x.athletic + y.athletic,
                comedy = x.comedy + y.comedy,
                conversation = x.conversation + y.conversation, attractive = x.attractive + y.attractive,
                literature = x.literature + y.literature, math = x.math + y.math, sociology = x.sociology + y.sociology,
                science = x.science + y.science, sports = x.sports + y.sports, art = x.art + y.art,
                hungry = x.hungry + y.hungry, fatigue = x.fatigue + y.fatigue, toilet = x.toilet + y.toilet,
                hygiene = x.hygiene + y.hygiene, loneliness = x.loneliness + y.loneliness, rLoneliness = x.rLoneliness + y.rLoneliness,
                fun = x.fun + y.fun, motivation = x.motivation + y.motivation
            };
        }

        public static CharacterStats operator +(CharacterStats x, float s)
        {
            return new CharacterStats()
            {
                logic = x.logic + s, language = x.language + s, aesthetics = x.aesthetics + s,
                social = x.social + s, athletic = x.athletic + s,
                comedy = x.comedy + s,
                conversation = x.conversation + s, attractive = x.attractive + s,
                literature = x.literature + s, math = x.math + s, sociology = x.sociology + s,
                science = x.science + s, sports = x.sports + s, art = x.art + s,
                hungry = x.hungry + s, fatigue = x.fatigue + s, toilet = x.toilet + s,
                hygiene = x.hygiene + s, loneliness = x.loneliness + s, rLoneliness = x.rLoneliness + s,
                fun = x.fun + s, motivation = x.motivation + s
            };
        }

        public static CharacterStats operator +(float s, CharacterStats x) => x + s;
        #endregion

        #region 곱셈 (*)
        public static CharacterStats operator *(CharacterStats x, CharacterStats y)
        {
            return new CharacterStats()
            {
                logic = x.logic * y.logic, language = x.language * y.language, aesthetics = x.aesthetics * y.aesthetics,
                social = x.social * y.social, athletic = x.athletic * y.athletic,
                comedy = x.comedy * y.comedy,
                conversation = x.conversation * y.conversation, attractive = x.attractive * y.attractive,
                literature = x.literature * y.literature, math = x.math * y.math, sociology = x.sociology * y.sociology,
                science = x.science * y.science, sports = x.sports * y.sports, art = x.art * y.art,
                hungry = x.hungry * y.hungry, fatigue = x.fatigue * y.fatigue, toilet = x.toilet * y.toilet,
                hygiene = x.hygiene * y.hygiene, loneliness = x.loneliness * y.loneliness, rLoneliness = x.rLoneliness * y.rLoneliness,
                fun = x.fun * y.fun, motivation = x.motivation * y.motivation
            };
        }

        public static CharacterStats operator *(CharacterStats x, float s)
        {
            return new CharacterStats()
            {
                logic = x.logic * s, language = x.language * s, aesthetics = x.aesthetics * s,
                social = x.social * s, athletic = x.athletic * s,
                comedy = x.comedy * s,
                conversation = x.conversation * s, attractive = x.attractive * s,
                literature = x.literature * s, math = x.math * s, sociology = x.sociology * s,
                science = x.science * s, sports = x.sports * s, art = x.art * s,
                hungry = x.hungry * s, fatigue = x.fatigue * s, toilet = x.toilet * s,
                hygiene = x.hygiene * s, loneliness = x.loneliness * s, rLoneliness = x.rLoneliness * s,
                fun = x.fun * s, motivation = x.motivation * s
            };
        }

        public static CharacterStats operator *(float s, CharacterStats x) => x * s;
        #endregion

        #region 뺄셈 (-)
        public static CharacterStats operator -(CharacterStats x, CharacterStats y)
        {
            return new CharacterStats()
            {
                logic = x.logic - y.logic, language = x.language - y.language, aesthetics = x.aesthetics - y.aesthetics,
                social = x.social - y.social, athletic = x.athletic - y.athletic,
                comedy = x.comedy - y.comedy,
                conversation = x.conversation - y.conversation, attractive = x.attractive - y.attractive,
                literature = x.literature - y.literature, math = x.math - y.math, sociology = x.sociology - y.sociology,
                science = x.science - y.science, sports = x.sports - y.sports, art = x.art - y.art,
                hungry = x.hungry - y.hungry, fatigue = x.fatigue - y.fatigue, toilet = x.toilet - y.toilet,
                hygiene = x.hygiene - y.hygiene, loneliness = x.loneliness - y.loneliness, rLoneliness = x.rLoneliness - y.rLoneliness,
                fun = x.fun - y.fun, motivation = x.motivation - y.motivation
            };
        }

        public static CharacterStats operator -(CharacterStats x, float s)
        {
            return new CharacterStats()
            {
                logic = x.logic - s, language = x.language - s, aesthetics = x.aesthetics - s,
                social = x.social - s, athletic = x.athletic - s,
                comedy = x.comedy - s, 
                conversation = x.conversation - s, attractive = x.attractive - s,
                literature = x.literature - s, math = x.math - s, sociology = x.sociology - s,
                science = x.science - s, sports = x.sports - s, art = x.art - s,
                hungry = x.hungry - s, fatigue = x.fatigue - s, toilet = x.toilet - s,
                hygiene = x.hygiene - s, loneliness = x.loneliness - s, rLoneliness = x.rLoneliness - s,
                fun = x.fun - s, motivation = x.motivation - s
            };
        }

        public static CharacterStats operator -(float s, CharacterStats x)
        {
            return new CharacterStats()
            {
                logic = s - x.logic, language = s - x.language, aesthetics = s - x.aesthetics,
                social = s - x.social, athletic = s - x.athletic,
                comedy = s - x.comedy,
                conversation = s - x.conversation, attractive = s - x.attractive,
                literature = s - x.literature, math = s - x.math, sociology = s - x.sociology,
                science = s - x.science, sports = s - x.sports, art = s - x.art,
                hungry = s - x.hungry, fatigue = s - x.fatigue, toilet = s - x.toilet,
                hygiene = s - x.hygiene, loneliness = s - x.loneliness, rLoneliness = s - x.rLoneliness,
                fun = s - x.fun, motivation = s - x.motivation
            };
        }
        #endregion

        #region 유틸리티 (Clamp)
        // 값을 최소(min) ~ 최대(max) 사이로 고정
        public CharacterStats Clamp(float min, float max)
        {
            return new CharacterStats()
            {
                logic = Math.Clamp(this.logic, min, max),
                language = Math.Clamp(this.language, min, max),
                aesthetics = Math.Clamp(this.aesthetics, min, max),
                social = Math.Clamp(this.social, min, max),
                athletic = Math.Clamp(this.athletic, min, max),
                
                comedy = Math.Clamp(this.comedy, min, max),
                conversation = Math.Clamp(this.conversation, min, max),
                attractive = Math.Clamp(this.attractive, min, max),
                
                literature = Math.Clamp(this.literature, min, max),
                math = Math.Clamp(this.math, min, max),
                sociology = Math.Clamp(this.sociology, min, max),
                science = Math.Clamp(this.science, min, max),
                sports = Math.Clamp(this.sports, min, max),
                art = Math.Clamp(this.art, min, max),
                
                hungry = Math.Clamp(this.hungry, min, max),
                fatigue = Math.Clamp(this.fatigue, min, max),
                toilet = Math.Clamp(this.toilet, min, max),
                hygiene = Math.Clamp(this.hygiene, min, max),
                loneliness = Math.Clamp(this.loneliness, min, max),
                rLoneliness = Math.Clamp(this.rLoneliness, min, max),
                fun = Math.Clamp(this.fun, min, max),
                motivation = Math.Clamp(this.motivation, min, max)
            };
        }
        #endregion

        #region 점수 계산

        public float SumENeeds()
        {
            return hungry + fatigue + toilet + hygiene;
        }

        public float SumRNeeds()
        {
            return loneliness + rLoneliness + fun;
        }

        public float SumGNeeds()
        {
            return motivation;
        }

        public float SumNeeds()
        {
            return SumENeeds() + SumRNeeds() + SumGNeeds();
        }
        
        #endregion

        public bool Equals(CharacterStats other)
        {
            return
                logic.Equals(other.logic) &&
                language.Equals(other.language) &&
                aesthetics.Equals(other.aesthetics) && 
                social.Equals(other.social) &&
                athletic.Equals(other.athletic)  && 
                comedy.Equals(other.comedy) && 
                conversation.Equals(other.conversation) && 
                attractive.Equals(other.attractive) &&
                literature.Equals(other.literature) && 
                math.Equals(other.math) && 
                sociology.Equals(other.sociology) && 
                science.Equals(other.science) &&
                sports.Equals(other.sports) && 
                art.Equals(other.art) && 
                hungry.Equals(other.hungry) &&
                fatigue.Equals(other.fatigue) && 
                toilet.Equals(other.toilet) && 
                hygiene.Equals(other.hygiene) && 
                loneliness.Equals(other.loneliness) &&
                rLoneliness.Equals(other.rLoneliness) &&
                fun.Equals(other.fun) &&
                motivation.Equals(other.motivation);
        }

        public override bool Equals(object obj)
        {
            return obj is CharacterStats other && Equals(other);
        }
        
        public float this[CharacterStatsType typeName]
{
    get
    {
        return typeName switch
        {
            CharacterStatsType.Logic => logic,
            CharacterStatsType.Language => language,
            CharacterStatsType.Aesthetics => aesthetics,
            CharacterStatsType.Social => social,
            CharacterStatsType.Athletic => athletic,
            /*-------------------------------*/
            CharacterStatsType.Comedy => comedy,
            CharacterStatsType.Conversation => conversation,
            CharacterStatsType.Attractive => attractive,
            /*-------------------------------*/
            CharacterStatsType.Literature => literature,
            CharacterStatsType.Math => math,
            CharacterStatsType.Sociology => sociology,
            CharacterStatsType.Science => science,
            CharacterStatsType.Sports => sports,
            CharacterStatsType.Art => art,
            /*-------------------------------*/
            CharacterStatsType.Hungry => hungry,
            CharacterStatsType.Fatigue => fatigue,
            CharacterStatsType.Toilet => toilet,
            CharacterStatsType.Hygiene => hygiene,
            CharacterStatsType.Loneliness => loneliness,
            CharacterStatsType.RLoneliness => rLoneliness,
            CharacterStatsType.Fun => fun,
            CharacterStatsType.Motivation => motivation,
            _ => 0
        };
    }
    set
    {
        switch (typeName)
        {
            case CharacterStatsType.Logic: logic = value; break;
            case CharacterStatsType.Language: language = value; break;
            case CharacterStatsType.Aesthetics: aesthetics = value; break;
            case CharacterStatsType.Social: social = value; break;
            case CharacterStatsType.Athletic: athletic = value; break;
            /*-------------------------------*/
            case CharacterStatsType.Comedy: comedy = value; break;
            case CharacterStatsType.Conversation: conversation = value; break;
            case CharacterStatsType.Attractive: attractive = value; break;
            /*-------------------------------*/
            case CharacterStatsType.Literature: literature = value; break;
            case CharacterStatsType.Math: math = value; break;
            case CharacterStatsType.Sociology: sociology = value; break;
            case CharacterStatsType.Science: science = value; break;
            case CharacterStatsType.Sports: sports = value; break;
            case CharacterStatsType.Art: art = value; break;
            /*-------------------------------*/
            case CharacterStatsType.Hungry: hungry = value; break;
            case CharacterStatsType.Fatigue: fatigue = value; break;
            case CharacterStatsType.Toilet: toilet = value; break;
            case CharacterStatsType.Hygiene: hygiene = value; break;
            case CharacterStatsType.Loneliness: loneliness = value; break;
            case CharacterStatsType.RLoneliness: rLoneliness = value; break;
            case CharacterStatsType.Fun: fun = value; break;
            case CharacterStatsType.Motivation: motivation = value; break;
            
            default:
                throw new ArgumentException($"알 수 없는 스테이터스 이름입니다: {typeName}");
        }
    }
}

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(logic);
            hashCode.Add(language);
            hashCode.Add(aesthetics);
            hashCode.Add(social);
            hashCode.Add(athletic);
            hashCode.Add(comedy);
            hashCode.Add(conversation);
            hashCode.Add(attractive);
            hashCode.Add(literature);
            hashCode.Add(math);
            hashCode.Add(sociology);
            hashCode.Add(science);
            hashCode.Add(sports);
            hashCode.Add(art);
            hashCode.Add(hungry);
            hashCode.Add(fatigue);
            hashCode.Add(toilet);
            hashCode.Add(hygiene);
            hashCode.Add(loneliness);
            hashCode.Add(rLoneliness);
            hashCode.Add(fun);
            hashCode.Add(motivation);
            return hashCode.ToHashCode();
        }
    }
}