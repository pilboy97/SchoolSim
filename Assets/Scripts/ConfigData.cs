using System;
using System.Collections.Generic;
using System.Linq;
using Game.Object.Character;
using Game.School;
using UnityEngine;

namespace Game
{
    public class ConfigData : Singleton<ConfigData>
    {
        public School.School schoolData;
        public bool isLogEnabled;

        public float eModifier = 1f;
        public float rModifier = 1f;
        public float gModifier = 1f;

        public float statsDecay = 0.1f;
        public float inertia = 5f;

        public float I_E_modifier = 5f;
        public float N_S_modifier = 5f;
        public float F_T_modifier = 5f;
        public float P_J_modifier = 5f;
        
        public float talk_baseFun = 1f;  // R-욕구 (결핍)
        public float talk_baseLoneliness = 1f;    // R-욕구 (관계)
        public float talk_baseRLoneliness = 1f;     // R-욕구 (로맨스)
        public float talk_baseTeach = 1f;     // G-욕구 (성장 - 양성 피드백 대상)
        public float talk_baseInfluence = 1f;
        public float talk_baseMotivation = 1f;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            
            if (schoolData == null) return;
            
            NameGenerator.Instance.Init();
            
            var d = ScriptableObject.CreateInstance<School.School>();
            d.schoolName = schoolData.schoolName;
            
            d.grp = new List<CharacterData>();
            
            d.maps = schoolData.maps.ToList();
            d.timeTable = schoolData.timeTable.ToList();
            
            d.characters = new List<CharacterData>();
            
            foreach (var ch in schoolData.characters)
            {
                var c = ScriptableObject.CreateInstance<CharacterData>();
                c.genData = ch.genData;
                
                c.Init();
                
                d.characters.Add(c);
            }

            d.classes = new List<Class>();
            foreach (var cl in schoolData.classes)
            {
                var c = ScriptableObject.CreateInstance<Class>();

                c.map = cl.map;
                c.className = cl.className;
                c.grp = new List<CharacterData>();

                foreach (var ch in d.characters)
                {
                    ch.classroom = c;
                }
                
                d.classes.Add(c);
            }
            
            d.grp.Clear();

            schoolData = d;
        }
    }
}