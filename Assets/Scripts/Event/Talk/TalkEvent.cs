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
        [SerializeField] private float baseLoneliness = 0.2f;  // R-욕구 (결핍)
        [SerializeField] private float baseFriendly = 1f;    // R-욕구 (관계)
        [SerializeField] private float baseRomance = 5f;     // R-욕구 (로맨스)
        [SerializeField] private float baseTeach = 0.2f;     // G-욕구 (성장 - 양성 피드백 대상)
        [SerializeField] private float baseInfluence = 2f;   // 평판 변화량

        [Header("State")]
        [SerializeField] private bool waitForSelect;
        [SerializeField] private bool alreadySelect;
        [SerializeField] public float selectTime = 1f;
        [SerializeField] public float curTime = 0f;
        [SerializeField] public StringTopicDict selected = new StringTopicDict();

        [Header("Influence")]
        public StringFloatDict scoreBySpeaker = new();
        public StringFloatDict shareOfInfluence = new();

        private (Topic topic, float score)[] _bestTopics;
        private int _bestSize = 4;

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

                return Mathf.Max(1, sum) / members.Count;
            }
        }

        public float DesiredShare => 1f / members.Count;
        public static List<Topic> Topics;

        public TalkEvent()
        {
            minMember = 2;
            maxMember = 4;

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

        private DeltaResult _result = new DeltaResult()
        {
            Stats = default,
            Relation = new ()
        };

        public override void CalcDeltaStats(Character c, ref DeltaResult result)
        {
            // 이 캐릭터에게 가장 점수가 높은 주제를 찾아 그 변화량을 반환
            Topic best = GetBestTopicForCharacter(c);

            result.Reset();
            
            CalcStatusDelta(c, best, ref result);

            if (members.Contains(c) && status == EventStatus.Run)
            {
                InternalCalcStatsDeltaWithShare(ref result, c);
            }
            
            c.CalcPersonalizedStatsDeltaOnReceive(ref result);
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

            foreach (var topic in Topics)
            {
                var t = topic;
                t.speaker = ch;

                _result.Reset();
                
                CalcStatusDelta(ch, t, ref _result);
                float score = controller.CalcScore(null, ref _result);

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

            if (scores.Count == 0) return default;

            var x = Random.SelectWithMultiplier(scores);
            return _bestTopics[x].topic;
        }

        private void InternalCalcStatsDeltaWithShare(ref DeltaResult delta, Character ch)
        {
            var modifier = shareOfInfluence.GetValueOrDefault(ch.ID) / DesiredShare;
            if (modifier < 0.3f) modifier = 0;
            
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
        }
        
        private void CalcStatusDelta(Character listener, Topic topic, ref DeltaResult result)
        {
            ref var resRel = ref result.Relation;
            ref var resStats = ref result.Stats;

            Character speaker = topic.speaker;
            if (speaker?.ID == null) return;

            switch (topic.type)
            {
                case Topic.Type.General:
                   CalcStatsDeltaForGeneral(listener, speaker, ref result);
                    break;

                case Topic.Type.Romance:
                   CalcStatsDeltaForRomance(listener, speaker, ref result);
                    break;

                case Topic.Type.Teach:
                  CalcStatsDeltaTeachEffect(listener, topic, ref result);
                    break;

                case Topic.Type.RelationUp:
                case Topic.Type.RelationDown:
                   CalcStatsDeltaReputationEffect(listener, topic, ref result);
                    break;
            }
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

                    _result.Reset();
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
            shareOfInfluence.Clear();

            float sum = 0;
            foreach (var member in members)
            {
                shareOfInfluence.TryAdd(member.ID, DesiredShare);
                scoreBySpeaker.TryAdd(member.ID, AverageScore);
                
                sum += scoreBySpeaker.GetValueOrDefault(member.ID);
            }

            foreach (var member in members)
            {
                shareOfInfluence[member.ID] = scoreBySpeaker[member.ID] / sum;
            }
        }

        private Dictionary<Character, DeltaResult> _applyCache = new ();
        
        private void ApplyAllSelectedTopics()
        {

            foreach (var member in members)
            {
                if (_applyCache.TryGetValue(member, out var value))
                {
                    value.Reset();
                    _applyCache[member] = value;
                }
                else
                {
                    _applyCache.TryAdd(member, new DeltaResult()
                    {
                        Stats = default,
                        Relation = new RelationFloatDict()
                    });
                }
            }
            
            foreach (var entry in selected)
            {
                Character speaker = ObjectManager.Instance.Find(entry.Key) as Character;
                if (speaker == null) continue;

                Topic topic = entry.Value;
                topic.speaker = speaker;
                scoreBySpeaker.TryAdd(speaker.ID, AverageScore);
                
                for (int i = 0; i < members.Count; i++)
                {
                    Character listener = members[i];

                    if (!_applyCache.TryGetValue(listener, out DeltaResult res))
                        res.Reset();
                    
                    CalcStatusDelta(listener, topic, ref res);
                    InternalCalcStatsDeltaWithShare(ref res, speaker);

                    scoreBySpeaker[speaker.ID] += listener.AI.CalcScore(ref res);

                    _applyCache.TryAdd(listener, res);
                }

            }
            
            foreach (var (ch, r) in _applyCache)
            {
                var res = r;
                ch.Receive(ref res, false);
            }
            
            UpdateShare();
        }
        
        private void CalcStatsDeltaForGeneral(Character listener, Character speaker, ref DeltaResult result)
        {
            ref var s = ref result.Stats;
            ref var r = ref result.Relation;
            
            if (listener.ID == speaker.ID)
            {
                for (int i = 0; i < members.Count; i++)
                {
                    var attr = listener.PersonalAttractionFrom(members[i]);
                    
                    if (members[i].ID == listener.ID) continue;
                    var f = new CharacterRelation()
                    {
                        ID = members[i].ID,
                        relType = CharacterRelation.Type.Friend,
                    };
                    
                    r.TryAdd(f, 0);
                    r[f] += baseFriendly * (2 * attr);
                    s.loneliness += baseLoneliness;
                }
            }
            else
            {
                var attr = listener.PersonalAttractionFrom(speaker);
                var f = new CharacterRelation()
                {
                    ID = speaker.ID,
                    relType = CharacterRelation.Type.Friend,
                };
                
                r.TryAdd(f, 0);
                r[f] += baseFriendly * (2 * attr);
                s.loneliness += baseLoneliness;
            }

            listener.CalcPersonalizedStatsDeltaOnReceive(r, ref result);
        }

        private void CalcStatsDeltaForRomance(Character listener, Character speaker, ref DeltaResult result)
        {
            ref var s = ref result.Stats;
            ref var r = ref result.Relation;
            
            
            if (listener.ID == speaker.ID)
            {
                for (int i = 0; i < members.Count; i++)
                {
                    if (members[i].ID == listener.ID) continue;
                    
                    var attr = listener.PersonalAttractionFrom(members[i]);
                    float attractionMod = Mathf.Max(0, 2 * attr - 1);
                    var val = baseRomance * attractionMod * attr;
                    
                    var ro = new CharacterRelation()
                    {
                        ID = members[i].ID,
                        relType = CharacterRelation.Type.Romance,
                    };
                    
                    r.TryAdd(ro, 0);
                    r[ro] += val;
                    s.rLoneliness+= baseRomance;
                }
            }
            else
            {
                var attr = listener.PersonalAttractionFrom(speaker);
                float attractionMod = Mathf.Max(0, 2 * attr - 1);
                var val = baseRomance * attractionMod * attr;
                var ro = new CharacterRelation()
                {
                    ID = speaker.ID,
                    relType = CharacterRelation.Type.Romance,
                };
                
                r.TryAdd(ro, 0);
                r[ro] += val;
            }

            listener.CalcPersonalizedStatsDeltaOnReceive(ref result);
        }
        
        private void CalcStatsDeltaTeachEffect(Character listener, Topic topic, ref DeltaResult result)
        {
            ref var s = ref result.Stats;
            ref var r = ref result.Relation;

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
                var f = new CharacterRelation()
                {
                    ID = speaker.ID,
                    relType = CharacterRelation.Type.Friend
                };
                
                r.TryAdd(f, 0);
                r[f] += effect;
            }

            listener.CalcPersonalizedStatsDeltaOnReceive(ref result);
        }

        private void  CalcStatsDeltaReputationEffect(Character listener, Topic topic, ref DeltaResult result)
        {
            ref var s = ref result.Stats;
            ref var r = ref result.Relation;
            
            Character target = ObjectManager.Instance.Find(topic.target.ID) as Character;
            var speaker = topic.speaker;

            if (target == null || topic.speaker.ID == topic.target.ID) return;

            bool isUp = topic.type == Topic.Type.RelationUp;
            float finalInf = isUp ? baseInfluence : -baseInfluence;
            
            if (listener.ID == speaker.ID)
            {
                if (!(isUp && !members.Contains(target)))
                {
                    var f = new CharacterRelation()
                    {
                        ID = topic.target.ID,
                        relType = CharacterRelation.Type.Friend
                    };
                    
                    r.TryAdd(f, 0);
                    r[f] += finalInf * 4;
                }

                foreach (var member in members)
                {
                    if (member.ID == speaker.ID || member.ID == target.ID) continue;
                    
                    var currentRel = listener.Data[new CharacterRelation { ID = topic.target.ID, relType = CharacterRelation.Type.Friend }];
                    var effect = (isUp ? 1 : -1) * (currentRel / 100f);
                    var f = new CharacterRelation()
                    {
                        ID = member.ID,
                        relType = CharacterRelation.Type.Friend
                    };
                    
                    r.TryAdd(f, 0);
                    r[f] += effect;
                }
            }
            else if (listener.ID == target.ID)
            {
                var f = new CharacterRelation()
                {
                    ID = topic.speaker.ID,
                    relType = CharacterRelation.Type.Friend
                };
                
               r.TryAdd(f, 0);
               r[f] += finalInf * 4;
            }
            else
            {
                var currentRelToSpeaker = listener.Data[new CharacterRelation { ID = speaker.ID, relType = CharacterRelation.Type.Friend }];
                var currentRelToTarget = listener.Data[new CharacterRelation { ID = target.ID, relType = CharacterRelation.Type.Friend }];
                
                var effectToSpeaker = (isUp ? 1 : -1) * (currentRelToTarget / 100f);
                var effectToTarget = finalInf * (currentRelToSpeaker / 100f);

                if (listener.IsRival(speaker) )
                {
                    effectToTarget *= 0.1f;
                }
                if (listener.IsFriend(speaker))
                {
                    effectToTarget *= 4;
                }

                var sp = new CharacterRelation()
                {
                    ID = speaker.ID,
                    relType = CharacterRelation.Type.Friend
                };
                var ta = new CharacterRelation()
                {
                    ID = target.ID,
                    relType = CharacterRelation.Type.Friend
                };
                
                r.TryAdd(sp, 0);
                r.TryAdd(ta, 0);

                r[sp] += effectToSpeaker;
                r[ta] += effectToTarget;
            }

            listener.CalcPersonalizedStatsDeltaOnReceive(ref result);
        }

        protected override void OnEnter(Character who)
        {
            base.OnEnter(who);
            Topic defaultTopic = Topics[0];
            defaultTopic.speaker = who;

            who.Busy = true;
            
            selected.TryAdd(who.ID, defaultTopic);
            scoreBySpeaker.TryAdd(who.ID, AverageScore);
            
            UpdateShare();
            
            if (who.ID == GameManager.Instance.Player?.ID) TalkWindow.Instance.Init(this);
        }

        protected override void OnLeave(Character who)
        {
            if (GameManager.IsQuitting) return;
            
            base.OnLeave(who);
            
            if (GameManager.Instance.Player != null && who.ID == GameManager.Instance.Player.ID) 
                TalkWindow.Instance?.Close();
            
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

        protected override void OnStart()
        {
            baseLoneliness = ConfigData.Instance.talk_baseLoneliness; // R-욕구 (결핍)
            baseFriendly = ConfigData.Instance.talk_baseFriendly; // R-욕구 (관계)
            baseRomance = ConfigData.Instance.talk_baseRomance; // R-욕구 (로맨스)
            baseTeach = ConfigData.Instance.talk_baseTeach; // G-욕구 (성장 - 양성 피드백 대상)
            baseInfluence = ConfigData.Instance.talk_baseInfluence; // 평판 변화량
            
            scoreBySpeaker.Clear();
            shareOfInfluence.Clear();
            
            UpdateShare();
        }

        public override bool Equals(Event other) => other is TalkEvent;
    }
}