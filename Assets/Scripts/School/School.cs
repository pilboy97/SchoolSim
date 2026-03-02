using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Game.Event;
using Game.Object;
using Game.Object.Character;
using Game.Room;
using Game.Task;
using Game.Time;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Game.School
{
    [CreateAssetMenu(menuName = "School/School")]
    public class School : ScriptableObject
    {
        [SerializeField] public string schoolName;
        [SerializeField] public int randomGenCharacter;
        [SerializeField] public List<CharacterData> characters = new();
        [SerializeReference] public List<Session> timeTable;
        [SerializeReference] public List<Class> classes = new();
        [SerializeReference] public List<RoomData> maps = new();

        public List<CharacterData> grp = new();

        private CharacterStatsType RandomSubject()
        {
            var subjects = new CharacterStatsType[]
            {
                CharacterStatsType.Literature,
                CharacterStatsType.Math,
                CharacterStatsType.Sociology,
                CharacterStatsType.Science,
                CharacterStatsType.Sports,
                CharacterStatsType.Art,
            };

            return Game.Random.Choose(subjects);
        }

        [Serializable]
        public class StartClassEffect : Effect
        {
            [SerializeField] public School school;
            public override void Do(Character subject, IInteractable other, Event.Event e)
            {
                school.StartClass(e);
            }

            public override bool Equals(Effect other)
            {
                if (other is not StartClassEffect t) return false;
                return t.school == school;
            }
        }
        [Serializable]
        public class EndClassEffect : Effect
        {
            [SerializeField] public School school;
            public override void Do(Character subject, IInteractable other, Event.Event e)
            {
                school.EndClass(e);
            }

            public override bool Equals(Effect other)
            {
                if (other is not EndClassEffect t) return false;
                return t.school == school;
            }
        }

        public void Init()
        {
            grp.Clear();
            foreach (var session in timeTable)
            {
                session.school = this;

                session.onStart = new StartClassEffect()
                {
                    school = this
                };
                session.onEnd = new EndClassEffect()
                {
                    school = this
                };

                ScheduleManager.Instance.AddSchedule(session);
            }
        }

        public void Enter(Character ch)
        {
            grp.Add(ch.Data);
        }
        public void Leave(Character ch)
        {
            grp.Remove(ch.Data);
        }
        
        public void StartClass(Event.Event e)
        {
            foreach (var chData in grp)
            {
                var character = ObjectManager.Instance.Find(chData.ID) as Character;
                character?.TryInviteMeAsync(GameManager.Instance.Global.Token, e, character, true).GetAwaiter().GetResult();
            }

            foreach (var cl in classes)
            {
                var cells = NavManager.Instance.WalkableCells.Where(x => x.z == cl.map.zIndex).ToList();

                foreach (var chData in cl.grp)
                {
                    var character = ObjectManager.Instance.Find(chData.ID) as Character;
                    character.CPosition = Random.Choose(cells);
                }
            }
        }

        public void EndClass(Event.Event e)
        {
        }
 
#if UNITY_EDITOR
        [Button("Create Time Table")]
        private void GenerateTimeTable()
        {
            timeTable = new();
            for (var i = 0; i < 5; i++)
            {
                var time = Calendar.ToTick(9, 0, 0);

                for (int j = 1; j <= 7; j++)
                {
                    var session = CreateInstance<Session>();
                    var begin = time;
                    var end = begin + Calendar.ToTick(1, 0, 0);

                    session.name = $"{Enum.GetName(typeof(DayFlag), (DayFlag)(1<<i))} {j} class";
                    var del = new CharacterStats
                    {
                        [RandomSubject()] = 1
                    };

                    session.effect = new AddDeltaEffect()
                    {
                        deltas = del
                    };

                    session.eventName = $"{j}th class";
                    session.start = begin;
                    session.end = end;
                    session.dayCond = (DayFlag)(1 << i);

                    timeTable.Add(session);

                    if (j == 4) time += Calendar.ToTick(1, 0, 0);
                    time += Calendar.ToTick(1, 10, 0);
                   
                    AssetDatabase.AddObjectToAsset(session, this);
                }

            }
            AssetDatabase.SaveAssets();
        }
#endif
    }
}