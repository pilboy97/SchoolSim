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

        [Header("Influence")]
        public StringFloatDict scoreBySpeaker = new();
        public StringFloatDict shareOfInfluence = new();

        private (Topic topic, float score)[] _bestTopics;
        private int _bestSize = 6;

        public float AverageScore
        {
            get
            {
                if (members.Count == 0) return 0;
                float sum = 0;
                foreach (var (_, v) in scoreBySpeaker)
                {
                    sum += v;
                }

                return sum / members.Count;
            }
        }

        public float DesiredShare => 1f / members.Count;
        public static List<Topic> Topics;

        // 결과값을 담기 위한 구조체 (할당 최적화)
        public struct DeltaResult
        {
            public CharacterStats Stats;
            public RelationFloatDict Relation;
        }

        public TalkEvent()
        {
            minMember = 2;
            maxMember = 8;
            
            eventName = "Talk";
            busy = false;
            _bestTopics = new (Topic topic, float score)[_bestSize];
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
            var result = CalcStatusDelta(c, best);

            if (members.Contains(c) && status == EventStatus.Run)
            {
                result = CalcStatsDeltaWithShare(result, c);
            }
            
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

            for (var i = 0; i < _bestSize; i++)
            {
                _bestTopics[i].topic = default;
                _bestTopics[i].score = -1;
            }

            for (int i = 0; i < Topics.Count; i++)
            {
                Topic t = Topics[i];
                t.speaker = ch;

                DeltaResult delta = CalcStatusDelta(ch, t);

                float score = controller.CalcScore((delta.Stats, delta.Relation), null);

                int j = _bestSize - 1;
                
                if (score > _bestTopics[j].score)
                {
                    _bestTopics[j] = (t, score);

                    while (j > 0 && _bestTopics[j].score > _bestTopics[j - 1].score)
                    {
                        (_bestTopics[j], _bestTopics[j - 1]) = (_bestTopics[j - 1], _bestTopics[j]);
                        j--;
                    }
                }
            }

            int cnt = 0;
            List<float> scores = new();
            for (cnt = 0; cnt < _bestSize; cnt++)
            {
                if (_bestTopics[cnt].score < 0) break;
                
                scores.Add(_bestTopics[cnt].score);
            }

            var x = Random.SelectWithMultiplier(scores);
            return _bestTopics[x].topic;
        }

        private DeltaResult CalcStatsDeltaWithShare(DeltaResult delta, Character ch)
        {
            var modifier = shareOfInfluence.GetValueOrDefault(ch.ID) / DesiredShare;
            delta.Stats *= modifier;

            if (delta.Relation != null)
            {
                // 딕셔너리에 들어있는 키들만 꺼내서 값만 수정 (O(N) 순회 방지)
                var keys = new List<CharacterRelation>(delta.Relation.Keys);
                foreach (var key in keys)
                {
                    delta.Relation[key] *= modifier;
                }
            }
            return delta;
        }
        
        private DeltaResult CalcStatusDelta(Character listener, Topic topic)
        {
            var resStats = new CharacterStats(){
                loneliness = baseLoneliness // 대화 자체로 고독감 해소
            };
            var resRel = new RelationFloatDict();

            Character speaker = topic.speaker;
            if (speaker == null) return default;

            switch (topic.type)
            {
                case Topic.Type.General:
                    (resStats, resRel) = CalcStatsDeltaForGeneral(listener, speaker);
                    break;

                case Topic.Type.Romance:
                    (resStats, resRel) = CalcStatsDeltaForRomance(listener, speaker);
                    break;

                case Topic.Type.Teach:
                    // G-욕구: 상대방의 숙련도가 높을수록 배울 점이 많아 점수가 높아짐
                    (resStats, resRel) = CalcStatsDeltaTeachEffect(listener, topic);
                    break;

                case Topic.Type.RelationUp:
                case Topic.Type.RelationDown:
                    (resStats, resRel) =  CalcStatsDeltaReputationEffect(listener, topic);
                    break;
            }
            
            return new DeltaResult { Stats = resStats, Relation = resRel };
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
                foreach (var member in members)
                {
                    member.Busy = true;
                }
                
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

            foreach (var member in members)
            {
                member.Busy = false;
            }

            alreadySelect = false;
            curTime = 0;
        }

        private void UpdateShare()
        {
            // 대화가 진행될수록 과거의 점수 비중을 살짝 줄임 (예: 20% 감쇠)
            var keys = new List<string>(scoreBySpeaker.Keys);
            foreach(var key in keys) {
                scoreBySpeaker[key] *= 0.8f; 
            }
    
            shareOfInfluence.Clear();

            float sum = 0;
            foreach (var member in members)
            {
                shareOfInfluence.TryAdd(member.ID, 0);
                scoreBySpeaker.TryAdd(member.ID, 0);
                sum +=  Mathf.Max(1, scoreBySpeaker.GetValueOrDefault(member.ID) );
            }

            foreach (var member in members)
            {
                shareOfInfluence[member.ID] = Mathf.Max(1, scoreBySpeaker[member.ID]) / sum;
                
            }
        }
        private void ApplyAllSelectedTopics()
        {
            foreach (var entry in selected)
            {
                Character speaker = ObjectManager.Instance.Find(entry.Key) as Character;
                if (speaker == null) continue;

                Topic topic = entry.Value;
                topic.speaker = speaker;
                
                for (int i = 0; i < members.Count; i++)
                {
                    Character listener = members[i];
    
                    // 1. 화자가 준비한 순수한 변화량 계산 (영향력 깎이기 전!)
                    DeltaResult rawRes = CalcStatusDelta(listener, topic);
    
                    scoreBySpeaker.TryAdd(speaker.ID, 0);

                    // 2. ★ 핵심 수정 ★ 
                    // 영향력이 곱해져서 딕셔너리가 오염되기 전에, '순수 델타(rawRes)'를 기반으로 화자의 가치를 평가합니다.
                    float relScore = listener.AI.CalcScore(rawRes.Relation, null);
                    float statsScore = listener.AI.CalcScore(
                        rawRes.Stats + listener.CalcPersonalizedStatsDeltaOnReceive(rawRes.Stats), null);
    
                    // 화자는 정당한 점수를 받습니다.
                    scoreBySpeaker[speaker.ID] += (relScore + statsScore);

                    // 3. 이제 현재 영향력(Share)을 곱해서 리스너에게 실제로 적용될 수치로 깎습니다.
                    DeltaResult finalRes = CalcStatsDeltaWithShare(rawRes, speaker);

                    // 4. 리스너에게는 깎인 수치(finalRes)만 실제 적용합니다. (반환되는 깎인 점수는 버림)
                    listener.Receive((finalRes.Stats, finalRes.Relation), false);
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
                    s.loneliness += baseFriendly;
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
                    
                    s.rLoneliness+= baseRomance;
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
                s[topic.knowledge] += effect;
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
            
            if (target == null || topic.speaker.ID == topic.target.ID) return default;

            bool isUp = topic.type == Topic.Type.RelationUp;
            float finalInf = isUp ? baseInfluence : -baseInfluence;
            
            if (listener.ID == speaker.ID)
            {
                if (!(isUp && !members.Contains(target)))
                {
                    r.Add(new CharacterRelation()
                    {
                        ID = topic.target.ID,
                        relType = CharacterRelation.Type.Friend
                    }, finalInf * 4);
                }

                foreach (var member in members)
                {
                    if (member.ID == speaker.ID || member.ID == target.ID) continue;
                    
                    var currentRel = listener.Data[new CharacterRelation { ID = topic.target.ID, relType = CharacterRelation.Type.Friend }];
                    var effect = (isUp ? 1 : -1) * (currentRel / 100f);
                    
                    r.Add(new CharacterRelation()
                    {
                        ID = member.ID,
                        relType = CharacterRelation.Type.Friend
                    }, effect);
                }
            }
            else if (listener.ID == target.ID)
            {
               r.Add(new CharacterRelation()
               {
                   ID = topic.speaker.ID,
                   relType = CharacterRelation.Type.Friend
               }, finalInf * 4);
            }
            else
            {
                var currentRelToSpeaker = listener.Data[new CharacterRelation { ID = topic.speaker.ID, relType = CharacterRelation.Type.Friend }];
                var currentRelToTarget = listener.Data[new CharacterRelation { ID = topic.target.ID, relType = CharacterRelation.Type.Friend }];
                
                var effectToSpeaker = (isUp ? 1 : -1) * (currentRelToTarget / 100f);
                var effectToTarget = finalInf * (currentRelToSpeaker / 100f);

                if (listener.IsRival(speaker) )
                {
                    return default;
                }
                if (listener.IsFriend(speaker))
                {
                    effectToTarget *= 4;
                }

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

            who.Busy = true;
            
            selected.TryAdd(who.ID, defaultTopic);
            scoreBySpeaker.TryAdd(who.ID, 0);
            scoreBySpeaker[who.ID] = AverageScore;
            
            UpdateShare();
            
            if (who.ID == GameManager.Instance.Player.ID) TalkWindow.Instance.Init(this);
        }

        protected override void OnLeave(Character who)
        {
                
            base.OnLeave(who);
            
            if (who.ID == GameManager.Instance.Player.ID) TalkWindow.Instance?.Close();
            
            selected.Remove(who.ID);
            scoreBySpeaker.Remove(who.ID); 
            shareOfInfluence.Remove(who.ID);
            
            UpdateShare();
        }
        
        protected override void OnDone()
        {
            base.OnDone();
            if (TalkWindow.Instance.EventID == id) TalkWindow.Instance.Close();
        }

        public override bool Equals(Event other) => other is TalkEvent;
    }
}