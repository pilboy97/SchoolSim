using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Object.Character
{
    [Serializable]
    public class CharacterGenData
    {
        public string charName;
        [SerializeField] public Vector3Int initPos = new Vector3Int(int.MaxValue, int.MaxValue, 0);
        [SerializeField] public Gender gender;
        [SerializeField] public MBTIComponent[] mbtiCond;
        [SerializeField] public int attractionLevel;
        [SerializeField] public List<CharacterData> friends = new();
        [SerializeField] public List<CharacterData> rivals = new();
    }
}