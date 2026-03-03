using System;
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

        public float inertia = 5f;

        public float I_E_modifier = 5f;
        public float N_S_modifier = 5f;
        public float F_T_modifier = 5f;
        public float P_J_modifier = 5f;
        
        public float talk_baseLoneliness = 0.2f;  // R-욕구 (결핍)
        public float talk_baseFriendly = 1f;    // R-욕구 (관계)
        public float talk_baseRomance = 5f;     // R-욕구 (로맨스)
        public float talk_baseTeach = 1f;     // G-욕구 (성장 - 양성 피드백 대상)
        public float talk_baseInfluence = 2f;  

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}