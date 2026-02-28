using System;
using System.Collections.Generic;
using Game.Object;
using Game.Object.Character;
using Game.Object.Character.AI;
using Game.Object.Character.Player;
using UnityEngine;
using UnityEngine.Diagnostics;

namespace Game.Event.Talk
{
    [Serializable]
    public class TalkEvent : Event
    {
        [Serializable]
        public class StringTopicDict : UnitySerializedDictionary<string, Topic> { }

        // --- 설정 필드 ---
        [Header("Settings")]
        [SerializeField] private float minMember = 2;
        [SerializeField] private float maxMember = 8;
        
        [Header("Weights")]
        [SerializeField] private float baseLoneliness = 5f;  // R-욕구 (결핍)
        [SerializeField] private float baseFriendly = 1f;    // R-욕구 (관계)
        [SerializeField] private float baseRomance = 1f;     // R-욕구 (로맨스)
        [SerializeField] private float baseTeach = 1f;     // G-욕구 (성장 - 양성 피드백 대상)
        [SerializeField] private float baseInfluence = 1f;   // 평판 변화량

        [Header("State")]
        [SerializeField] private bool waitForSelect;
        [SerializeField] private bool alreadySelect;
        [SerializeField] public float selectTime = 5f;
        [SerializeField] public float curTime = 0f;
        [SerializeField] public StringTopicDict selected = new StringTopicDict();

        public StringFloatDict scoreBySpeaker = new();
        public StringFloatDict shareOfMember = new();
        
        public static List<Topic> Topics;

        // 결과값을 담기 위한 구조체 (할당 최적화)
        public struct DeltaResult
        {
            public CharacterStats Stats;
            public RelationFloatDict Relation;
        }

        public TalkEvent()
        {
            eventName = "Talk";
            busy = false;
        }

        // --- 초기화 로직 ---
        public static void StaticInit()
        {
            if (Topics != null) return;
            Topics = new List<Topic>();

            // 1. 일반 및 로맨스 주제
            Topics.Add(new Topic { type = Topic.Type.General });
            Topics.Add(new Topic { type = Topic.Type.Romance });

            // 2. 기술 & 공부 관련 주제

            for (var skill = CharacterStatsType.SkillBegin + 1; skill < CharacterStatsType.SkillEnd; skill++)
            {
                Topics.Add(new Topic { knowledge = skill, type = Topic.Type.Teach });
            }
            for (var subject = CharacterStatsType.SubjectBegin + 1; subject < CharacterStatsType.SubjectEnd; subject++)
            {
                Topics.Add(new Topic { knowledge = subject, type = Topic.Type.Teach });
            }

            // 3. 인물 관계 주제 (Up/Down)
            var characters = ObjectManager.Instance.Characters;
            foreach (var character in characters)
            {
                Topics.Add(new Topic { target = character, type = Topic.Type.RelationUp });
                Topics.Add(new Topic { target = character, type = Topic.Type.RelationDown });
            }
        }

        // --- 핵심 계산 로직 ---

        public override (CharacterStats, RelationFloatDict) CalcDeltaStats(Character c)
        {
            // 이 캐릭터에게 가장 점수가 높은 주제를 찾아 그 변화량을 반환
            Topic best = GetBestTopicForCharacter(c);
            var result = InternalCalculateDelta(c, best);

            var ret = result.Stats;
            var rel = result.Relation;

            ret += c.CalcPersonalizedStatsDeltaOnReceive(rel);

            return (ret, rel);
        }

        private Topic GetBestTopicForCharacter(Character ch)
        {
            AIControl controller = null;
            if (ch.ControllerType == ControllerType.Player)
            {
                controller = ch.AI;
            }
            else
            {
                controller = (AIControl)ch.controller;
            }

            if (controller == null) return selected.GetValueOrDefault(ch.ID);
            
            float maxScore = float.NegativeInfinity;
            Topic bestTopic = default;

            for (int i = 0; i < Topics.Count; i++)
            {
                Topic t = Topics[i];
                t.speaker = ch;

                DeltaResult delta = InternalCalculateDelta(ch, t);

                // 여기서 우리가 만든 E, R, G(양성피드백) 공식이 적용된 점수를 계산함
                float score = controller.CalcScore((delta.Stats, delta.Relation), null);

                if (score > maxScore)
                {
                    maxScore = score;
                    bestTopic = t;
                }
            }
            return bestTopic;
        }

        private DeltaResult InternalCalculateDelta(Character listener, Topic topic)
        {
            var resStatus = new CharacterStats(){
                loneliness = baseLoneliness // 대화 자체로 고독감 해소
            };
            var resRel = new RelationFloatDict();

            Character speaker = topic.speaker;
            if (speaker == null) return default;

            float attraction = listener.PersonalAttractionFrom(speaker);

            switch (topic.type)
            {
                case Topic.Type.General:
                    (resStatus, resRel) = CalcStatsDeltaForGeneral(listener, speaker);
                    break;

                case Topic.Type.Romance:
                    (resStatus, resRel) = CalcStatsDeltaForRomance(listener, speaker);
                    break;

                case Topic.Type.Teach:
                    // G-욕구: 상대방의 숙련도가 높을수록 배울 점이 많아 점수가 높아짐
                    float speakerSkill = speaker.Data[topic.knowledge];
                    float learningValue = (speakerSkill / 100f + 1) * baseTeach;
                    
                    resStatus[topic.knowledge] = learningValue;
                    (resStatus, resRel) = CalcStatsDeltaTeachEffect(listener, topic);
                    break;

                case Topic.Type.RelationUp:
                case Topic.Type.RelationDown:
                    (resStatus, resRel) =  CalcStatsDeltaReputationEffect(listener, topic);
                    break;
            }

            return new DeltaResult { Stats = resStatus, Relation = resRel };
        }

        // --- 실행 및 상태 관리 ---

        protected override void OnRun()
        {
            base.OnRun();
            if (status == EventStatus.Done) return;

            curTime += UnityEngine.Time.deltaTime;

            // 1. 플레이어 선택 대기 로직
            if (waitForSelect)
            {
                if (!TalkWindow.Instance.isChoose && curTime < selectTime) return;

                Character player = GameManager.Instance.Player;
                Topic pTopic = TalkWindow.Instance.Selected;
                pTopic.speaker = player;
                selected[player.ID] = pTopic;

                waitForSelect = false;
                alreadySelect = true;
                return;
            }

            // 2. 주제 선정 단계 (모든 NPC가 동시에 생각)
            if (!alreadySelect)
            {
                Character player = GameManager.Instance.Player;
                bool playerInEvent = false;

                for (int i = 0; i < members.Count; i++)
                {
                    Character m = members[i];
                    if (m == player)
                    {
                        playerInEvent = true;
                    }
                    
                    selected[m.ID] = GetBestTopicForCharacter(m);
                }

                if (playerInEvent) waitForSelect = true;
                else alreadySelect = true;
                return;
            }

            // 3. 결과 적용 단계 (시간 종료 후)
            if (curTime < selectTime) return;

            ApplyAllSelectedTopics();

            alreadySelect = false;
            curTime = 0;
        }

        private void UpdateShare()
        {
            shareOfMember.Clear();

            float sum = 0;
            foreach (var member in members)
            {
                sum +=  Mathf.Max(1, scoreBySpeaker[member.ID]);
            }

            foreach (var member in members)
            {
                shareOfMember.TryAdd(member.ID, 0);
                shareOfMember[member.ID] = Mathf.Max(1, scoreBySpeaker[member.ID]) / sum;
            }
        }
        private void ApplyAllSelectedTopics()
        {
            // selected 딕셔너리 순회 (LINQ 없이 Enumerator 사용)
            var enumerator = selected.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var entry = enumerator.Current;
                Character speaker = ObjectManager.Instance.Find(entry.Key) as Character;
                if (speaker == null) continue;

                Topic topic = entry.Value;
                topic.speaker = speaker;

                for (int i = 0; i < members.Count; i++)
                {
                    Character listener = members[i];
                    DeltaResult res = InternalCalculateDelta(listener, topic);

                    scoreBySpeaker.TryAdd(speaker.ID, 0);
                    scoreBySpeaker[speaker.ID] += listener.Receive((res.Stats, res.Relation), false);
                }
            }
            
            UpdateShare();
        }
        
        private (CharacterStats, RelationFloatDict) CalcStatsDeltaForGeneral(Character listener, Character speaker)
        {
            var s = new CharacterStats();
            var r = new RelationFloatDict();
            
            if (listener.ID == speaker.ID)
            {
                for (int i = 0; i < members.Count; i++)
                {
                    if (members[i].ID == listener.ID) continue;
                    
                    r.Add(new CharacterRelation()
                    {
                        ID = members[i].ID,
                        relType = CharacterRelation.Type.Friend,
                    }, baseFriendly);
                }
            }
            else
            {
                r.Add(new CharacterRelation()
                {
                    ID = speaker.ID,
                    relType = CharacterRelation.Type.Friend,
                }, baseFriendly);
            }

            s += listener.CalcPersonalizedStatsDeltaOnReceive(r);
            return (s, r);
        }

        private (CharacterStats, RelationFloatDict)  CalcStatsDeltaForRomance(Character listener, Character speaker)
        {
            var s = new CharacterStats();
            var r = new RelationFloatDict();
            
            
            if (listener.ID == speaker.ID)
            {
                for (int i = 0; i < members.Count; i++)
                {
                    if (members[i].ID == listener.ID) continue;
                    
                    var attr = speaker.PersonalAttractionFrom(members[i]);
                    float attractionMod = Mathf.Max(0, 2 * attr - 1);
                    var val = baseRomance * attractionMod;
                    
                    r.Add(new CharacterRelation()
                    {
                        ID = members[i].ID,
                        relType = CharacterRelation.Type.Romance,
                    }, val);
                }
            }
            else
            {
                var attr = listener.PersonalAttractionFrom(speaker);
                float attractionMod = Mathf.Max(0, 2 * attr - 1);
                var val = baseRomance * attractionMod;
                r.Add(new CharacterRelation()
                {
                    ID = speaker.ID,
                    relType = CharacterRelation.Type.Romance,
                }, val);
            }

            s += listener.CalcPersonalizedStatsDeltaOnReceive(r);
            return (s, r);
        }
        
        private (CharacterStats, RelationFloatDict)  CalcStatsDeltaTeachEffect(Character listener, Topic topic)
        {
            var s = new CharacterStats();
            var r = new RelationFloatDict();

            var speaker = topic.speaker;

            float speakerSkill = speaker.Data[topic.knowledge];
            var effect = (speakerSkill / 100 + 1) * baseTeach;
            
            if (listener.ID == speaker.ID)
            {
                s += new CharacterStats()
                {
                    motivation = effect
                };
            }
            else
            {
                s[topic.knowledge] += effect;
                r.Add(new CharacterRelation()
                {
                    ID = speaker.ID,
                    relType = CharacterRelation.Type.Friend
                }, effect);
            }

            s += listener.CalcPersonalizedStatsDeltaOnReceive(r);
            return (s, r);
        }

        private (CharacterStats, RelationFloatDict)  CalcStatsDeltaReputationEffect(Character listener, Topic topic)
        {
            var s = new CharacterStats();
            var r = new RelationFloatDict();
            
            Character target = ObjectManager.Instance.Find(topic.target.ID) as Character;
            var speaker = topic.speaker;
            
            if (target == null || topic.speaker.ID == topic.target.ID) return (s, r);

            bool isUp = topic.type == Topic.Type.RelationUp;
            float finalInf = isUp ? baseInfluence : -baseInfluence;

            if (listener.ID == speaker.ID)
            {
                r.Add(new CharacterRelation()
                {
                    ID = topic.target.ID,
                    relType = CharacterRelation.Type.Friend
                }, finalInf * 3);
            }
            else if (listener.ID == target.ID)
            {
               r.Add(new CharacterRelation()
               {
                   ID = topic.speaker.ID,
                   relType = CharacterRelation.Type.Friend
               }, finalInf * 3);
            }
            else
            {
                var currentRelToSpeaker = listener.Data[new CharacterRelation { ID = topic.target.ID, relType = CharacterRelation.Type.Friend }];
                var currentRelToTarget = listener.Data[new CharacterRelation { ID = topic.target.ID, relType = CharacterRelation.Type.Friend }];
                
                var effectToSpeaker = (isUp ? 1 : -1) * (currentRelToTarget / 100f);
                var effectToTarget = finalInf * (currentRelToSpeaker / 100f);

                r.Add(new CharacterRelation()
                {
                    ID = speaker.ID,
                    relType = CharacterRelation.Type.Friend
                }, effectToSpeaker);
                r.Add(new CharacterRelation()
                {
                    ID = topic.target.ID,
                    relType = CharacterRelation.Type.Friend
                }, effectToTarget);
            }

            s += listener.CalcPersonalizedStatsDeltaOnReceive(r);
            return (s, r);
        }

        protected override void OnEnter(Character who)
        {
            base.OnEnter(who);
            Topic defaultTopic = Topics[0];
            defaultTopic.speaker = who;
            selected.TryAdd(who.ID, defaultTopic);
            scoreBySpeaker.TryAdd(who.ID, 0);
            
            UpdateShare();
            
            if (who.ID == GameManager.Instance.Player.ID) TalkWindow.Instance.Init(this);
        }

        protected override void OnLeave(Character who)
        {
            base.OnLeave(who);
            
            if (who.ID == GameManager.Instance.Player.ID) TalkWindow.Instance.Close();
            selected.Remove(who.ID);
            scoreBySpeaker[who.ID] = 0;
            
            UpdateShare();
        }

        protected override bool CheckRun() => members.Count >= minMember && members.Count < maxMember;
        protected override bool CheckInvite(Character who) => true;
        
        protected override void OnDone()
        {
            base.OnDone();
            if (TalkWindow.Instance.EventID == id) TalkWindow.Instance.Close();
        }

        public override bool Equals(Event other) => other is TalkEvent;
    }
}