using System;
using System.Collections.Generic;
using System.Linq;
using Game.Map;
using Game.School;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Object.Character
{
    public enum Gender
    {
        Male,
        Female
    }

    public enum MBTIComponent
    {
        E,
        I,
        S,
        N,
        T,
        F,
        J,
        P,
    }

    public enum MBTI
    {
        ESTJ = 0b0000,
        ESTP = 0b0001,
        ESFJ = 0b0010,
        ESFP = 0b0011,
        ENTJ = 0b0100,
        ENTP = 0b0101,
        ENFJ = 0b0110,
        ENFP = 0b0111,
        ISTJ = 0b1000,
        ISTP = 0b1001,
        ISFJ = 0b1010,
        ISFP = 0b1011,
        INTJ = 0b1100,
        INTP = 0b1101,
        INFJ = 0b1110,
        INFP = 0b1111,
    }

    public static class MBTIHelper
    {
        public static MBTIComponent[] ToComponents(this MBTI mbti)
        {
            var ret = new MBTIComponent[4];

            if (((int)mbti & 0b1000) != 0) ret[0] = MBTIComponent.I;
            else ret[0] = MBTIComponent.E;
            if (((int)mbti & 0b0100) != 0) ret[1] = MBTIComponent.N;
            else ret[1] = MBTIComponent.S;
            if (((int)mbti & 0b0010) != 0) ret[2] = MBTIComponent.F;
            else ret[2] = MBTIComponent.T;
            if (((int)mbti & 0b0001) != 0) ret[3] = MBTIComponent.P;
            else ret[3] = MBTIComponent.J;

            return ret;
        }

        public static MBTI FromComponents(this MBTIComponent[] components)
        {
            UnityEngine.Debug.Assert(components.Length != 4, "Wrong MBTI");

            int ret = 0;
            foreach (var comp in components)
            {
                switch (comp)
                {
                    case MBTIComponent.I:
                        ret |= 0b1000;
                        break;
                    case MBTIComponent.N:
                        ret |= 0b0100;
                        break;
                    case MBTIComponent.F:
                        ret |= 0b0010;
                        break;
                    case MBTIComponent.P:
                        ret |= 0b0001;
                        break;
                }
            }

            return (MBTI)ret;
        }

        public static MBTI RandomMBTI()
        {
            return Random.ChooseEnum<MBTI>();
        }

        public static MBTI GenerateMBTI(MBTIComponent[] cond)
        {
            var fix = new bool[4];
            int ret = 0;

            foreach (var comp in cond)
            {
                switch (comp)
                {
                    case MBTIComponent.E or MBTIComponent.I:
                        fix[0] = true;

                        ret |= (comp == MBTIComponent.I) ? 0b1000 : 0;
                        break;
                    case MBTIComponent.S or MBTIComponent.N:
                        fix[1] = true;

                        ret |= (comp == MBTIComponent.N) ? 0b0100 : 0;
                        break;
                    case MBTIComponent.T or MBTIComponent.F:
                        fix[2] = true;

                        ret |= (comp == MBTIComponent.F) ? 0b0010 : 0;
                        break;
                    case MBTIComponent.J or MBTIComponent.P:
                        fix[3] = true;

                        ret |= (comp == MBTIComponent.P) ? 0b0001 : 0;
                        break;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                if (!fix[i])
                {
                    int r = UnityEngine.Random.Range(0, 2);

                    ret |= (r != 0) ? (1 << i) : 0;
                }
            }

            return (MBTI)ret;
        }

        public static bool CheckComponent(this MBTI mbti, MBTIComponent comp)
        {
            int val = (int)mbti;
            switch (comp)
            {
                case MBTIComponent.I: return (val & 0b1000) != 0; // 8이면 True
                case MBTIComponent.E: return (val & 0b1000) == 0; // 0이면 True
                case MBTIComponent.N: return (val & 0b0100) != 0;
                case MBTIComponent.S: return (val & 0b0100) == 0;
                case MBTIComponent.F: return (val & 0b0010) != 0;
                case MBTIComponent.T: return (val & 0b0010) == 0;
                case MBTIComponent.P: return (val & 0b0001) != 0;
                case MBTIComponent.J: return (val & 0b0001) == 0;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public static string ToString(this MBTI mbti)
        {
            return Enum.GetName(typeof(MBTI), mbti);
        }
    }

    [CreateAssetMenu(menuName = "Object/Character")]
    public class CharacterData : ScriptableObject
    {
        [Header("basic information")]
        [SerializeField] public string ID;
        [SerializeField] public Vector3 position;
        [SerializeField] public string charName;
        [SerializeField] public Gender gender;
        [SerializeField] public MBTI mbti;
        
        [Header("attraction")]
        [SerializeField] public Vector3 beauty;
        [SerializeField] public float attraction;
        [SerializeField] public float e;

        [Header("ai score")]
        [SerializeField] public float eModifier = 1f;
        [SerializeField] public float rModifier = 1f;
        [SerializeField] public float gModifier = 1f;

        [Header("status")]
        [SerializeField] public CharacterStats stats = new();
        [SerializeField] public RelationFloatDict relations = new();
        [SerializeField] public List<CharacterData> friends = new();
        [SerializeField] public List<CharacterData> rivals = new();

        [Header("Generate Character Data")] 
        [SerializeReference]
        private CharacterGenData genData = null;
            
        [Header("etc")]
        [SerializeField] public Class classroom;
        [SerializeField] public string eventID;
        [SerializeField] private DistributionNormalDistribution friendDistribution = new DistributionNormalDistribution(50, 10);
        [SerializeField] private DistributionNormalDistribution rivalDistribution = new DistributionNormalDistribution(-50,10);

        public CharacterData(string id = null)
        {
            Init();

            if (id != null)
                ID = id;
        }

        public void Init()
        {
            ID ??= IHasID.GenerateID();
            stats = new();
            relations = new();
            beauty = new Vector3(
                    UnityEngine.Random.value,
                    UnityEngine.Random.value,
                    UnityEngine.Random.value
                )
                .normalized;

            gender = Random.ChooseEnum<Gender>();
            mbti = MBTIHelper.RandomMBTI();
            charName = NameGenerator.RandomName(gender);

            position = MapController.Instance.CellToWorld(NavManager.Instance.RandomPos);
            attraction = DistributionNormalDistribution.GetTruncatedNormal(-2f, 2f);

            e = new DistributionNormalDistribution(0.5f, 0.1f).Sample;

            eModifier = 1;
            rModifier = 1;
            gModifier = 1;

            eventID = "";

            relations = new RelationFloatDict();
            rivals = new List<CharacterData>();
            friends = new List<CharacterData>();
            
            if (genData != null)
            {
                GenerateCharacter();
            }
        }

        public void GenerateCharacter()
        {
            if (genData == null) return;

            stats = default;

            if (genData.attractionLevel != 0)
            {
                var alevel = genData.attractionLevel - 3;

                float pmin, pmax;
                if (alevel >= 0)
                {
                    pmin = alevel;
                    pmax = float.PositiveInfinity;
                }
                else
                {
                    pmin = float.NegativeInfinity;
                    pmax = alevel + 1;
                }

                var attr = DistributionNormalDistribution.GetTruncatedNormal(pmin, pmax);

                attraction = attr;
            }

            if (genData.initPos.x != int.MaxValue)
                position = MapController.Instance.CellToWorld(genData.initPos);

            gender = genData.gender;

            mbti = MBTIHelper.GenerateMBTI(genData.mbtiCond);

            friends = genData.friends.ToList();
            rivals = genData.rivals.ToList();

            foreach (var friend in friends)
            {
                relations.TryAdd(new CharacterRelation()
                {
                    ID = friend.ID,
                    relType = CharacterRelation.Type.Friend
                }, friendDistribution.Sample);
            }
            foreach (var rival in rivals)
            {
                relations.TryAdd(new CharacterRelation()
                {
                    ID = rival.ID,
                    relType = CharacterRelation.Type.Friend
                }, rivalDistribution.Sample);
            }
            
            charName = genData.charName;
            eventID = "";
        }

        public void Receive(CharacterStats x)
        {
            stats += x;
        }
        
        
        public void Set(CharacterRelation rel, float v)
        {
            relations[rel] = v;
            relations[rel] = Mathf.Clamp(relations[rel], -100, 100);
        }

        public void Receive(CharacterRelation rel, float v)
        {
            if (v > 8) 
                UnityEngine.Debug.Log($"TOO BIG {v}");
            
            this[rel] += v;
        }

        public float this[CharacterRelation key]
        {
            get => relations.GetValueOrDefault(key, 0);
            set => Set(key, value);
        }

        public float this[CharacterStatsType statsType]
        {
            get => stats[statsType];
            set => stats[statsType] = value;
        }

        public float GetVar(string name)
        {
            return GameManager.Instance.GetVar(ID, name);
        }

        public void SetVar(string name, float value)
        {
            GameManager.Instance.SetVar(ID, name, value);
        }

        [Button("generate")]
        private void GenerateCharacterOnEditor()
        {
            GameManager.Instance.InitOnEditorMode(false);
            Init();

            GenerateCharacter();
        }
    }
}