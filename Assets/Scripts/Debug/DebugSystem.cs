using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Object;
using Game.Object.Character;
using Game.Time;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Debug
{
    [Serializable]
    public struct CharacterDataForLog
    {
        [Serializable]
        public struct Relation
        {
            public CharacterRelation rel;
            public float val;
        }

        public int tick;
        public string id;
        public MBTI mbti;
        public string charName;
        public string eventId;
        public float attraction;
        public float e;
        public List<string> friends;
        public List<string> rivals;
        public CharacterStats stats;
        public List<Relation> relations;

        public static CharacterDataForLog FromCharacterData(int tick, CharacterData data)
        {
            CharacterDataForLog ret = new();

            ret.tick = tick;
            ret.id = data.ID;
            ret.mbti = data.mbti;
            ret.charName = data.charName;
            ret.attraction = data.attraction;
            ret.e = data.e;
            ret.friends = data.friends.Select(x => x.ID).ToList();
            ret.rivals = data.rivals.Select(x => x.ID).ToList();
            ret.stats = data.stats;
            ret.relations = data.relations.Select(tup => new Relation()
            {
                rel = tup.Key,
                val = tup.Value
            }).ToList();


            return ret;
        }
    }

    [Serializable]
    public struct DebugData
    {
        public List<CharacterDataForLog> chDatas;
    }

    public class DebugSystem : Singleton<DebugSystem>
    {
        [ShowInInspector] private bool IsLogEnabled => ConfigData.Instance.isLogEnabled;
        [SerializeField] private string logFolder;
        [SerializeField] private string logFile = "log.json";
        
        private int _lastLoggedTick = -1;

        [SerializeField] private DebugData data;

        public string LogPath => Path.Combine(logFolder, logFile);

        private void Awake()
        {
            logFolder = Path.Combine(Application.persistentDataPath, "Log");

            Directory.CreateDirectory(logFolder);

            data.chDatas = new();
        }

        private void LateUpdate()
        {
            if (!IsLogEnabled) return;
            
            int currentTick = TimeManager.Instance.Ticks;

            if (currentTick == _lastLoggedTick) return;
    
            _lastLoggedTick = currentTick;

            foreach (var ch in ObjectManager.Instance.Characters)
            {
                data.chDatas.Add(CharacterDataForLog.FromCharacterData(currentTick,ch.Data));
            }
        }

        private void OnDestroy()
        {
            var str = JsonUtility.ToJson(data);
            File.WriteAllText(LogPath,str);

            UnityEngine.Debug.Log($"{LogPath}");
        }
    }
}