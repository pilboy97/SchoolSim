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
        public CharacterGenData genData = new ();
            
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

            position = MapController.Instance?.CellToWorld(NavManager.Instance?.RandomPos ?? Vector3Int.zero) ?? Vector3.zero;
            attraction = DistributionNormalDistribution.GetTruncatedNormal(-2f, 2f);

            e = new DistributionNormalDistribution(0.5f, 0.1f).Sample;

            eModifier = 1;
            rModifier = 1;
            gModifier = 1;

            eventID = "";

            relations = new RelationFloatDict();
            rivals = new List<CharacterData>();
            friends = new List<CharacterData>();

            friendDistribution = new DistributionNormalDistribution(50f, 10f);
            rivalDistribution = new DistributionNormalDistribution(-50f, 10f);

            eModifier = ConfigData.Instance.eModifier;
            rModifier = ConfigData.Instance.rModifier;
            gModifier = ConfigData.Instance.gModifier;
            
            
            if (genData != null)
            {
                GenerateCharacter();
            }
        }

        public void GenerateCharacter()
        {
            if (genData == null) return;

            ID ??= IHasID.GenerateID();
            stats = genData.stats;

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

        public void Receive(CharacterRelation rel, float v)
        {
            this[rel] += v;
        }

        public void Receive(DeltaResult result)
        {
            Receive(result.Stats);
            foreach (var (k, v) in result.Relation)
            {
                Receive(k, v);
            }
        }
        
        public void Set(CharacterRelation rel, float v)
        {
            relations[rel] = v;
            relations[rel] = Mathf.Clamp(relations[rel], -100, 100);
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