// using System;
// using System.Collections.Generic;
// using System.Xml.Resolvers;
// using Game.Object;
// using Game.Object.Character;
// using UnityEngine;
//
// namespace Game.Event.Talk
// {
//     [Serializable]
//     public class TalkEvent : Event
//     {
//         [Serializable]
//         public class StringTopicDict : UnitySerializedDictionary<string, Topic>
//         {
//             
//         }
//         
//         [SerializeField] private float minMember = 2;
//         [SerializeField] private float maxMember = 8;
//         [SerializeField] private float loneliness = 5;
//         [SerializeField] private float friendly = 1;
//         
//         [SerializeField] private float romance = 1;
//         [SerializeField] private float skill = 1;
//         [SerializeField] private float influence = 1;
//
//         [SerializeField] private bool waitForSelect;
//         [SerializeField] private bool alreadySelect;
//         
//         [SerializeField] public float selectTime = 5f;
//         [SerializeField] public float curTime = 0f;
//
//         [SerializeField] public StringTopicDict selected = new();
//         
//         public TalkEvent()
//         {
//             eventName = "Talk";
//             busy = false;
//         }
//
//         public static void StaticInit()
//         {
//             Topics = new();
//             
//             Topics.Add(new Topic()
//             {
//                 type = Topic.Type.General
//             });
//             Topics.Add(new Topic()
//             {
//                 type = Topic.Type.Romance
//             });
//             
//             for (var it = CharacterStatus.SkillBegin + 1; it != CharacterStatus.SkillEnd; it++)
//             {
//                 Topics.Add(new Topic()
//                 {
//                     topic = it,
//                     type = Topic.Type.Skill
//                 });
//             }
//
//             for (var it = CharacterStatus.SubjectBegin + 1; it != CharacterStatus.SubjectEnd; it++)
//             {
//                 Topics.Add(new Topic()
//                 {
//                     topic = it,
//                     type = Topic.Type.Subject
//                 });
//             }
//
//             foreach (var ch in ObjectManager.Instance.Characters)
//             {
//                 Topics.Add(new Topic()
//                 {
//                     target = ch,
//                     type = Topic.Type.RelationUp
//                 });
//                 Topics.Add(new Topic()
//                 {
//                     target = ch,
//                     type = Topic.Type.RelationDown
//                 });
//             }
//         }
//         
//         private CharacterStatusDeltaFactory Default => new CharacterStatusDeltaFactory(new StatusFloatDict()
//         {
//             {CharacterStatus.Loneliness, loneliness},
//         });
//
//         public static List<Topic> Topics;
//
//         private (CharacterStatusDeltaFactory, RelationFloatDict) CalcStatsDeltaStats(Character c, Topic topic)
//         {
//             var ret = Default;
//             var retRel = new RelationFloatDict();
//             
//             Character target;
//             float value;
//
//             CharacterRelation rel = default;
//             float attr;
//             
//             switch (topic.type)
//             {
//                 case Topic.Type.General:
//                     if (c.ID == topic.subject.ID)
//                     {
//                         foreach (var ch in members)
//                         {
//                             if (ch.ID == c.ID) continue;
//
//                             rel = new CharacterRelation()
//                             {
//                                 relType = CharacterRelation.Type.Friend,
//                                 ID = ch.ID
//                             };
//                             
//                             ret.Add(c.CalcPersonalizedStatsDeltaOnReceiveStatsDelta(rel, friendly));
//                         }
//                     }
//                     else
//                     {
//                         attr = c.PersonalAttractionFrom(topic.subject);
//                         rel = new CharacterRelation()
//                         {
//                             relType = CharacterRelation.Type.Friend,
//                             ID = topic.subject.ID
//                         };
//                             
//                         ret.Add(c.CalcPersonalizedStatsDeltaOnReceiveStatsDelta(rel, friendly));
//                         retRel.Add(rel, friendly * 2 * attr);
//                     }
//                 
//                     break;
//                 
//                 case Topic.Type.Romance: 
//                     attr = c.PersonalAttractionFrom(topic.subject);
//                     target = topic.subject;
//
//                     if (c.ID == target.ID)
//                     {
//                         foreach (var ch in members)
//                         {
//                             attr = c.PersonalAttractionFrom(ch);
//                             var attrMod = Mathf.Max(0, 2 * attr - 1);
//                 
//                             value = Mathf.Max(romance * attrMod, 1);
//                             ret.Add(CharacterStatus.RLoneliness, value * attrMod);
//                         }
//
//                         break;
//                     }
//                 
//                     rel = new CharacterRelation()
//                     {
//                         relType = CharacterRelation.Type.Romance,
//                         ID = topic.subject.ID
//                     };
//                             
//                     ret.Add(c.CalcPersonalizedStatsDeltaOnReceiveStatsDelta(rel, romance));
//                     retRel.Add(rel, romance * (2 * attr - 1));
//                     
//                     break;
//                 
//                 case Topic.Type.Skill:
//                 case Topic.Type.Subject:
//                     var hisSkill = topic.subject.Data[topic.topic];
//                     value = (hisSkill / 100f + 1) * skill;
//                     
//                     rel = new CharacterRelation()
//                     {
//                         relType = CharacterRelation.Type.Friend,
//                         ID = topic.subject.ID
//                     };
//                             
//                     ret.Add(topic.topic, value);
//                     ret.Add(c.CalcPersonalizedStatsDeltaOnReceiveStatsDelta(rel, value));
//                     retRel.Add(rel, value);
//                 
//                     break;
//                 case Topic.Type.RelationUp:
//                 {
//                     if (topic.subject.ID == topic.target.ID) return (null, null);
//                     
//                     target = ObjectManager.Instance.Find(topic.target.ID) as Character;
//
//                     if (target == null) return (null, null);
//
//                     if (c.ID == target.ID)
//                     {
//                         attr = c.PersonalAttractionFrom(topic.subject);
//                         rel = new CharacterRelation()
//                         {
//                             relType = CharacterRelation.Type.Friend,
//                             ID = topic.subject.ID
//                         };
//                             
//                         ret.Add(c.CalcPersonalizedStatsDeltaOnReceiveStatsDelta(rel, friendly));
//                         retRel.Add(rel, friendly * 4 * attr);
//                     }
//                     else
//                     {
//                         var myRelation = c.Data[new CharacterRelation()
//                         {
//                             ID = topic.target.ID,
//                             relType = CharacterRelation.Type.Friend,
//                         }];
//
//                         var meToSub = myRelation / 100f;
//                         var meToSubRel = new CharacterRelation()
//                         {
//                             relType = CharacterRelation.Type.Friend,
//                             ID = topic.subject.ID
//                         };
//
//                         var meToTarget = influence;
//                         var meToTargetRel = new CharacterRelation()
//                         {
//                             relType = CharacterRelation.Type.Friend,
//                             ID = topic.subject.ID
//                         };
//
//                         retRel.TryAdd(meToSubRel, 0);
//                         retRel[meToSubRel] += meToSub;
//                         ret.Add(c.CalcPersonalizedStatsDeltaOnReceiveStatsDelta(meToSubRel, meToSub));
//
//                         retRel.TryAdd(meToTargetRel, 0);
//                         retRel[meToTargetRel] += meToTarget;
//                         ret.Add(c.CalcPersonalizedStatsDeltaOnReceiveStatsDelta(meToTargetRel, meToTarget));
//                     }
//
//                     break;
//                 }
//                 case Topic.Type.RelationDown:
//                 {
//                     if (topic.subject.ID == topic.target.ID) return (null, null);
//                     
//                     target = ObjectManager.Instance.Find(topic.target.ID) as Character;
//
//                     if (target == null) return (null, null);
//
//                     if (c.ID == target.ID)
//                     {
//                         attr = c.PersonalAttractionFrom(topic.subject);
//                         rel = new CharacterRelation()
//                         {
//                             relType = CharacterRelation.Type.Friend,
//                             ID = topic.subject.ID
//                         };
//                             
//                         ret.Add(c.CalcPersonalizedStatsDeltaOnReceiveStatsDelta(rel, -friendly));
//                         retRel.Add(rel, -friendly * 4 * attr);
//                     }
//                     else
//                     {
//                         var myRelation = c.Data[new CharacterRelation()
//                         {
//                             ID = topic.target.ID,
//                             relType = CharacterRelation.Type.Friend,
//                         }];
//
//                         var meToSub = -myRelation / 100f;
//                         var meToSubRel = new CharacterRelation()
//                         {
//                             relType = CharacterRelation.Type.Friend,
//                             ID = topic.subject.ID
//                         };
//
//                         var meToTarget = -influence;
//                         var meToTargetRel = new CharacterRelation()
//                         {
//                             relType = CharacterRelation.Type.Friend,
//                             ID = topic.subject.ID
//                         };
//
//                         retRel.TryAdd(meToSubRel, 0);
//                         retRel[meToSubRel] += meToSub;
//                         ret.Add(c.CalcPersonalizedStatsDeltaOnReceiveStatsDelta(meToSubRel, meToSub));
//
//                         retRel.TryAdd(meToTargetRel, 0);
//                         retRel[meToTargetRel] += meToTarget;
//                         ret.Add(c.CalcPersonalizedStatsDeltaOnReceiveStatsDelta(meToTargetRel, meToTarget));
//                     }
//                     
//                     break;
//                 }
//                 default:
//                     throw new ArgumentOutOfRangeException();
//             }
//
//             return (ret, retRel);
//         }
//
//         public override (CharacterStatusDeltaFactory, RelationFloatDict) CalcDeltaStats(Character c)
//         {
//             (CharacterStatusDeltaFactory, RelationFloatDict) ret = (null, null);
//             
//             var maxScore = float.NegativeInfinity;
//             foreach (var topic in Topics)
//             {
//                 var t = topic;
//                 t.subject = c;
//                     
//                 var delta = CalcStatsDeltaStats(c, t);
//                 var score = c.CalcScore(delta, null);
//
//                 if (maxScore < score)
//                 {
//                     maxScore = score;
//                     ret = delta;
//                 }
//             }
//
//             return ret;
//         }
//
//         protected override void OnRun()
//         {
//             base.OnRun();
//             
//             if (status == EventStatus.Done) return;
//
//             curTime += UnityEngine.Time.deltaTime;
//             if (waitForSelect)
//             {
//                 if (!TalkWindow.Instance.isChoose && curTime < selectTime) return;
//
//                 Topic playerTopic = TalkWindow.Instance.Selected;
//                 playerTopic.subject = GameManager.Instance.Player;
//
//                 selected[GameManager.Instance.Player.ID] = playerTopic;
//
//                 waitForSelect = false;
//                 alreadySelect = true;
//
//                 return;
//             }
//
//
//             if (!alreadySelect)
//             {
//                 foreach (var ch in members)
//                 {
//                     if (ch.ID == GameManager.Instance.Player.ID)
//                     {
//                         selected[ch.ID] = TalkWindow.Instance.Selected;
//                         continue;
//                     }
//
//                     var maxScore = float.NegativeInfinity;
//                     Topic maxTopic = Topics[0];
//                     maxTopic.subject = ch;
//                 
//                     foreach (var topic in Topics)
//                     {
//                         var t = topic;
//                         t.subject = ch;
//
//                         var delta = CalcStatsDeltaStats(ch, t);
//                         var score = ch.CalcScore(delta, null);
//
//                         if (maxScore < score)
//                         {
//                             maxScore = score;
//                             maxTopic = t;
//                         }
//                     }
//
//                     selected[ch.ID] = maxTopic;
//                 }
//                 
//                 if (members.Contains(GameManager.Instance.Player))
//                 {
//                     waitForSelect = true;
//                 }
//
//                 return;
//             }
//
//             if (curTime < selectTime) return;
//
//             foreach (var (cid, topic) in selected)
//             {
//                 var ch = (Character)ObjectManager.Instance.Find(cid);
//                 var t = topic;
//                 t.subject = ch;
//
//                 foreach (var ch2 in members)
//                 {
//                     var delta = CalcStatsDeltaStats(ch2, t);
//
//                     (delta.Item1 ?? new()).Apply(ch, false);
//
//                     foreach (var (rel, v) in delta.Item2 ?? new())
//                     {
//                         ch2.Apply(rel, v);
//                         ch2.CalcPersonalizedStatsDeltaOnReceiveStatsDelta(rel, v).Apply(ch2);
//                     }
//                 }
//             }
//
//             alreadySelect = false;
//             curTime = 0;
//         }
//
//         protected override void OnEnter(Character who)
//         {
//             base.OnEnter(who);
//             
//             var topic = Topics[0];
//             topic.subject = who;
//             selected.TryAdd(who.ID, topic);
//             
//             if (who.ID == GameManager.Instance.Player.ID)
//             {
//                 TalkWindow.Instance.Init(this);
//             }
//         }
//
//         protected override void OnLeave(Character who)
//         {
//             base.OnLeave(who);
//             
//             if (who.ID == GameManager.Instance.Player.ID)
//             {
//                 TalkWindow.Instance.Close();
//             }
//             
//             selected.Remove(who.ID);
//         }
//
//         protected override bool CheckRun()
//         {
//             return members.Count >= minMember && members.Count < maxMember;
//         }
//
//         protected override bool CheckInvite(Character who)
//         {
//             return true;
//         }
//
//         protected override void OnDone()
//         {
//             base.OnDone();
//             
//             if (TalkWindow.Instance.EventID == id) TalkWindow.Instance.Close();
//         }
//
//         public override bool Equals(Event other)
//         {
//             return other is TalkEvent;
//         }
//     }
// }
using System;
using System.Collections.Generic;
using Game.Object;
using Game.Object.Character;
using Game.Object.Character.AI;
using UnityEngine;

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
        [SerializeField] private float loneliness = 5f;  // R-욕구 (결핍)
        [SerializeField] private float friendly = 1f;    // R-욕구 (관계)
        [SerializeField] private float romance = 1f;     // R-욕구 (로맨스)
        [SerializeField] private float skill = 1.5f;     // G-욕구 (성장 - 양성 피드백 대상)
        [SerializeField] private float influence = 1f;   // 평판 변화량

        [Header("State")]
        [SerializeField] private bool waitForSelect;
        [SerializeField] private bool alreadySelect;
        [SerializeField] public float selectTime = 5f;
        [SerializeField] public float curTime = 0f;
        [SerializeField] public StringTopicDict selected = new StringTopicDict();

        public static List<Topic> Topics;

        // 결과값을 담기 위한 구조체 (할당 최적화)
        public struct DeltaResult
        {
            public CharacterStatusDeltaFactory status;
            public RelationFloatDict relation;
            public bool IsValid => status != null;
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

            // 2. 기술 관련 주제 (G-욕구 관련)
            for (var it = CharacterStatus.SkillBegin + 1; it < CharacterStatus.SkillEnd; it++)
            {
                Topics.Add(new Topic { topic = it, type = Topic.Type.Skill });
            }

            // 3. 관심 주제
            for (var it = CharacterStatus.SubjectBegin + 1; it < CharacterStatus.SubjectEnd; it++)
            {
                Topics.Add(new Topic { topic = it, type = Topic.Type.Subject });
            }

            // 4. 인물 관계 주제 (Up/Down)
            var characters = ObjectManager.Instance.Characters;
            foreach (var character in characters)
            {
                Topics.Add(new Topic { target = character, type = Topic.Type.RelationUp });
                Topics.Add(new Topic { target = character, type = Topic.Type.RelationDown });
            }
        }

        // --- 핵심 계산 로직 ---

        public override (CharacterStatusDeltaFactory, RelationFloatDict) CalcDeltaStats(Character c)
        {
            // 이 캐릭터에게 가장 점수가 높은 주제를 찾아 그 변화량을 반환
            Topic best = GetBestTopicForCharacter(c);
            if (best.type == Topic.Type.None) return (null, null);

            var result = InternalCalculateDelta(c, best);
            return (result.status, result.relation);
        }

        private Topic GetBestTopicForCharacter(Character ch)
        {
            if (ch.ControllerType != ControllerType.AI) return default;

            var controller = ch.controller as AIControl;
            if (controller == null) return default;
            
            float maxScore = float.NegativeInfinity;
            Topic bestTopic = default;

            for (int i = 0; i < Topics.Count; i++)
            {
                Topic t = Topics[i];
                t.subject = ch;

                DeltaResult delta = InternalCalculateDelta(ch, t);
                if (!delta.IsValid) continue;

                // 여기서 우리가 만든 E, R, G(양성피드백) 공식이 적용된 점수를 계산함
                float score = controller.CalcScore(delta.status, null);

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
            var resStatus = new CharacterStatusDeltaFactory(new StatusFloatDict {
                { CharacterStatus.Loneliness, loneliness } // 대화 자체로 고독감 해소
            });
            var resRel = new RelationFloatDict();

            Character speaker = topic.subject;
            if (speaker == null) return default;

            float attraction = listener.PersonalAttractionFrom(speaker);

            switch (topic.type)
            {
                case Topic.Type.General:
                    ApplyGeneralEffect(listener, speaker, friendly, attraction, resStatus, resRel);
                    break;

                case Topic.Type.Romance:
                    ApplyRomanceEffect(listener, speaker, romance, attraction, resStatus, resRel);
                    break;

                case Topic.Type.Skill:
                case Topic.Type.Subject:
                    // G-욕구: 상대방의 숙련도가 높을수록 배울 점이 많아 점수가 높아짐
                    float speakerSkill = speaker.Data[topic.topic];
                    float learningValue = (speakerSkill / 100f + 1) * skill;
                    resStatus.Add(topic.topic, learningValue);
                    AddRelationValue(listener, speaker.ID, CharacterRelation.Type.Friend, learningValue, resStatus, resRel);
                    break;

                case Topic.Type.RelationUp:
                case Topic.Type.RelationDown:
                    ApplyReputationEffect(listener, topic, influence, resStatus, resRel);
                    break;
            }

            return new DeltaResult { status = resStatus, relation = resRel };
        }

        // --- 실행 및 상태 관리 ---

        protected override void OnRun()
        {
            if (status == EventStatus.Done) return;

            curTime += UnityEngine.Time.deltaTime;

            // 1. 플레이어 선택 대기 로직
            if (waitForSelect)
            {
                if (!TalkWindow.Instance.isChoose && curTime < selectTime) return;

                Character player = GameManager.Instance.Player;
                Topic pTopic = TalkWindow.Instance.Selected;
                pTopic.subject = player;
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
                        continue;
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
                topic.subject = speaker;

                for (int i = 0; i < members.Count; i++)
                {
                    Character listener = members[i];
                    DeltaResult res = InternalCalculateDelta(listener, topic);

                    // 1. 상태 적용
                    if (res.status != null) res.status.Apply(listener, false);

                    // 2. 관계 적용
                    if (res.relation != null)
                    {
                        var relEnum = res.relation.GetEnumerator();
                        while (relEnum.MoveNext())
                        {
                            var relPair = relEnum.Current;
                            listener.Apply(relPair.Key, relPair.Value);
                            // 관계 변화에 따른 심리적 피드백 적용
                            listener.CalcPersonalizedStatsDeltaOnReceiveStatsDelta(relPair.Key, relPair.Value).Apply(listener);
                        }
                    }
                }
            }
        }

        // --- 헬퍼 메서드 (중복 제거) ---

        private void AddRelationValue(Character me, string targetID, CharacterRelation.Type type, float val, CharacterStatusDeltaFactory sDelta, RelationFloatDict rDelta)
        {
            var rel = new CharacterRelation { ID = targetID, relType = type };
            sDelta.Add(me.CalcPersonalizedStatsDeltaOnReceiveStatsDelta(rel, val));
            rDelta.TryAdd(rel, 0);
            rDelta[rel] += val;
        }

        private void ApplyGeneralEffect(Character listener, Character speaker, float baseVal, float attr, CharacterStatusDeltaFactory sDelta, RelationFloatDict rDelta)
        {
            if (listener.ID == speaker.ID)
            {
                for (int i = 0; i < members.Count; i++)
                {
                    if (members[i].ID == listener.ID) continue;
                    AddRelationValue(listener, members[i].ID, CharacterRelation.Type.Friend, baseVal, sDelta, rDelta);
                }
            }
            else
            {
                AddRelationValue(listener, speaker.ID, CharacterRelation.Type.Friend, baseVal, sDelta, rDelta);
                // 호감도가 높을수록 관계 상승폭 증가
                var rel = new CharacterRelation { ID = speaker.ID, relType = CharacterRelation.Type.Friend };
                rDelta[rel] += baseVal * attr; 
            }
        }

        private void ApplyRomanceEffect(Character listener, Character speaker, float baseVal, float attr, CharacterStatusDeltaFactory sDelta, RelationFloatDict rDelta)
        {
            float attractionMod = Mathf.Max(0, 2 * attr - 1);
            if (listener.ID == speaker.ID)
            {
                for (int i = 0; i < members.Count; i++)
                {
                    if (members[i].ID == listener.ID) continue;
                    sDelta.Add(CharacterStatus.RLoneliness, Mathf.Max(baseVal * attractionMod, 1));
                }
            }
            else
            {
                AddRelationValue(listener, speaker.ID, CharacterRelation.Type.Romance, baseVal, sDelta, rDelta);
                var rel = new CharacterRelation { ID = speaker.ID, relType = CharacterRelation.Type.Romance };
                rDelta[rel] += baseVal * attractionMod;
            }
        }

        private void ApplyReputationEffect(Character listener, Topic topic, float baseInfluence, CharacterStatusDeltaFactory sDelta, RelationFloatDict rDelta)
        {
            Character target = ObjectManager.Instance.Find(topic.target.ID) as Character;
            if (target == null || topic.subject.ID == topic.target.ID) return;

            bool isUp = topic.type == Topic.Type.RelationUp;
            float finalInf = isUp ? baseInfluence : -baseInfluence;

            if (listener.ID == target.ID)
            {
                AddRelationValue(listener, topic.subject.ID, CharacterRelation.Type.Friend, finalInf * 4 * listener.PersonalAttractionFrom(topic.subject), sDelta, rDelta);
            }
            else
            {
                // 제3자에 대한 평판 전해듣기
                float currentRel = listener.Data[new CharacterRelation { ID = topic.target.ID, relType = CharacterRelation.Type.Friend }];
                float effect = (isUp ? 1 : -1) * (currentRel / 100f);
                AddRelationValue(listener, topic.subject.ID, CharacterRelation.Type.Friend, effect, sDelta, rDelta);
                AddRelationValue(listener, topic.target.ID, CharacterRelation.Type.Friend, finalInf, sDelta, rDelta);
            }
        }

        // --- 이벤트 생명주기 ---

        protected override void OnEnter(Character who)
        {
            base.OnEnter(who);
            Topic defaultTopic = Topics[0];
            defaultTopic.subject = who;
            selected.TryAdd(who.ID, defaultTopic);
            
            if (who.ID == GameManager.Instance.Player.ID) TalkWindow.Instance.Init(this);
        }

        protected override void OnLeave(Character who)
        {
            base.OnLeave(who);
            if (who.ID == GameManager.Instance.Player.ID) TalkWindow.Instance.Close();
            selected.Remove(who.ID);
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