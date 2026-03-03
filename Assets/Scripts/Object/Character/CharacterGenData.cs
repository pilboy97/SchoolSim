using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Object.Character
{
    [Serializable]
    public class CharacterGenData
    {
        public string ID;
        public string charName;
        
        [SerializeField] public CharacterStats stats;
        [SerializeField] public Vector3Int initPos = new Vector3Int(int.MaxValue, int.MaxValue, 0);
        [SerializeField] public Gender gender;
        [SerializeField] public MBTIComponent[] mbtiCond;
        [SerializeField] public int attractionLevel;
        [SerializeField] public List<CharacterData> friends = new();
        [SerializeField] public List<CharacterData> rivals = new();
        
        public void Init()
        {
            stats = new();

            attractionLevel = 0;

            gender = Random.ChooseEnum<Gender>();
            charName = NameGenerator.RandomName(gender);

            rivals = new List<CharacterData>();
            friends = new List<CharacterData>();

            mbtiCond = new MBTIComponent[4]
            {
                MBTIComponent.None,
                MBTIComponent.None,
                MBTIComponent.None,
                MBTIComponent.None,
            };
        }
    }
}