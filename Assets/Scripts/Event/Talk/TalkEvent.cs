using System;
using System.Collections.Generic;
using Game.Object;
using Game.Object.Character;
using Game.Object.Character.AI;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Event.Talk
{
    [Serializable]
    public class TalkEvent : Event
    {
        [Serializable]
        public class StringTopicDict : UnitySerializedDictionary<string, Topic> { }
        
        [Header("Weights")]
        [ShowInInspector] public float BaseFun => ConfigData.Instance.talk_baseFun; // R-욕구 (결핍)
        [ShowInInspector] public float BaseLoneliness => ConfigData.Instance.talk_baseLoneliness; // R-욕구 (관계)
        [ShowInInspector] public float BaseRLoneliness => ConfigData.Instance.talk_baseRLoneliness; // R-욕구 (로맨스)
        [ShowInInspector] public float BaseTeach => ConfigData.Instance.talk_baseTeach; // G-욕구 (성장 - 양성 피드백 대상)
        [ShowInInspector] public float BaseInfluence => ConfigData.Instance.talk_baseInfluence; // 평판 변화량
        [ShowInInspector] public float BaseMotivation => ConfigData.Instance.talk_baseMotivation;  // 관계 변화량

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
        
        List<Character> _kill = new ();

        public TalkEvent()
        {
            minMember = 2;
            maxMember = 4;

            eventName = "Talk";
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
                
                float score = controller.CalcScore(null, _result);

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
        {;
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
            
            speaker.CalcPersonalizedStatsDeltaOnReceive(result.Relation, ref result);
            speaker.CalcPersonalizedStatsDeltaOnReceive(result.Stats, ref result);
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

            foreach (var member in members)
            {
                member.Busy = false;
            }
            
            ApplyAllSelectedTopics();

            _kill.Clear();
            foreach (var ch in members)
            {
                if(shareOfInfluence[ch.ID] <  0.5f)
                {
                    _kill.Add(ch);
                }
            }

            foreach (var ch in _kill)
            {
                Leave(ch);
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
            // 캐시 초기화
            foreach (var member in members)
            {
                if (_applyCache.TryGetValue(member, out var value))
                {
                    value.Reset();
                    _applyCache[member] = value;
                }
                else
                {
                    _applyCache.TryAdd(member,
                        new DeltaResult() { Stats = default, Relation = new RelationFloatDict() });
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

                    // 1. 해당 스피커 전용의 '깨끗한' DeltaResult 생성
                    DeltaResult speakerDelta = new DeltaResult()
                        { Stats = default, Relation = new RelationFloatDict() };

                    // 2. 스피커 1명의 순수 변화량만 계산
                    CalcStatusDelta(listener, topic, ref speakerDelta);

                    // 3. 이 스피커의 변화량에만 지분율(Share) 곱셈 적용
                    InternalCalcStatsDeltaWithShare(ref speakerDelta, speaker);

                    // 4. 안전하게 계산된 데이터를 리스너의 캐시에 합산
                    var res = _applyCache[listener];
                    res.Stats += speakerDelta.Stats;
                    if (speakerDelta.Relation != null)
                    {
                        foreach (var (rel, v) in speakerDelta.Relation)
                        {
                            res.Relation.TryAdd(rel, 0);
                            res.Relation[rel] += v;
                        }
                    }

                    _applyCache[listener] = res;

                    // AI 점수 산정 (scoreBySpeaker 누적)
                    scoreBySpeaker[speaker.ID] += listener.AI.CalcScore(null, speakerDelta);
                }
            }

            // 5. 모든 스피커의 턴이 끝난 후, 누적된 관계망을 한 번에 스탯으로 변환!
            foreach (var (ch, r) in _applyCache)
            {
                var res = r;
                ch.Receive(res, false);
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
                    var attrMod = listener.AttractionModifier(members[i]);
                    attrMod = (attrMod > 0) ? attrMod : 0;
                    
                    if (members[i].ID == listener.ID) continue;
                    var f = new CharacterRelation()
                    {
                        ID = members[i].ID,
                        relType = CharacterRelation.Type.Friend,
                    };
                    
                    r.TryAdd(f, 0);
                    r[f] += BaseInfluence * attrMod;
                }
            }
            else
            {
                var attrMod = listener.AttractionModifier(speaker);
                attrMod = (attrMod > 0) ? attrMod : 0;
                var f = new CharacterRelation()
                {
                    ID = speaker.ID,
                    relType = CharacterRelation.Type.Friend,
                };
                
                r.TryAdd(f, 0);
                r[f] += BaseInfluence * attrMod;
            }

            s[CharacterStatsType.Fun] += BaseFun;
            s[CharacterStatsType.Loneliness] += BaseLoneliness;
        }

        private void CalcStatsDeltaForRomance(Character listener, Character speaker, ref DeltaResult result)
        {
            ref var s = ref result.Stats;
            ref var r = ref result.Relation;
            
            
            if (listener.ID == speaker.ID)
            {
                for (int i = 0; i < members.Count; i++)
                {
                    if (members[i].ID == speaker.ID) continue;
                    
                    var attrMod = speaker.AttractionModifier(members[i]);
                    float attractionMod = Mathf.Max(0, attrMod);
                    var val = BaseInfluence * attractionMod;
                    
                    var ro = new CharacterRelation()
                    {
                        ID = members[i].ID,
                        relType = CharacterRelation.Type.Romance,
                    };
                    
                    r.TryAdd(ro, 0);
                    r[ro] += val;
                }
            }
            else
            {
                var attrMod = listener.AttractionModifier(speaker);
                var val = BaseInfluence * attrMod;
                var ro = new CharacterRelation()
                {
                    ID = speaker.ID,
                    relType = CharacterRelation.Type.Romance,
                };
                
                r.TryAdd(ro, 0);
                r[ro] += val;
            }

            s[CharacterStatsType.Fun] += BaseFun;
            s[CharacterStatsType.RLoneliness] += BaseRLoneliness;
        }
        
        private void CalcStatsDeltaTeachEffect(Character listener, Topic topic, ref DeltaResult result)
        {
            ref var s = ref result.Stats;
            ref var r = ref result.Relation;

            var speaker = topic.speaker;

            float speakerSkill = speaker.Data[topic.knowledge];
            var effect = (speakerSkill / 100) * BaseTeach;
            var rel = (speakerSkill / 100) * BaseInfluence;
            
            if (listener.ID == speaker.ID)
            {
                for (int i = 0; i < members.Count; i++)
                {
                    if (members[i].ID == speaker.ID) continue;
                    
                    var attr = speaker.PersonalAttractionFrom(members[i]);
                    rel *= attr;
                    
                    var f = new CharacterRelation()
                    {
                        ID = members[i].ID,
                        relType = CharacterRelation.Type.Friend,
                    };
                    
                    r.TryAdd(f, 0);
                    r[f] += rel;
                }
                
                s[CharacterStatsType.Motivation] += effect * BaseMotivation;
            }
            else
            {
                var attr = listener.PersonalAttractionFrom(speaker);
                s[topic.knowledge] += effect;
                var f = new CharacterRelation()
                {
                    ID = speaker.ID,
                    relType = CharacterRelation.Type.Friend
                };
                
                r.TryAdd(f, 0);
                r[f] += rel * attr;
                s[CharacterStatsType.Motivation] += (speakerSkill / 100) * BaseMotivation;
            }
        }

        private void  CalcStatsDeltaReputationEffect(Character listener, Topic topic, ref DeltaResult result)
        {
            ref var s = ref result.Stats;
            ref var r = ref result.Relation;
            
            Character target = ObjectManager.Instance.Find(topic.target.ID) as Character;
            var speaker = topic.speaker;

            if (target == null || topic.speaker.ID == topic.target.ID) return;
            
            bool isUp = topic.type == Topic.Type.RelationUp;
            float finalInf = isUp ? BaseInfluence : -BaseInfluence;
            
            if (listener.ID == speaker.ID)
            {
                var attrMod = listener.AttractionModifier(target);
                attrMod = (attrMod > 0) ? attrMod : 0;
                
                var f = new CharacterRelation()
                {
                    ID = topic.target.ID,
                    relType = CharacterRelation.Type.Friend
                };
                
                r.TryAdd(f, 0);
                r[f] += finalInf * 4 * attrMod;

                foreach (var member in members)
                {
                    if (member.ID == speaker.ID || member.ID == target.ID) continue;
                    
                    var currentRel = member.Data[new CharacterRelation { ID = topic.target.ID, relType = CharacterRelation.Type.Friend }];
                    var effect = finalInf * (currentRel / 100f);
                    
                    attrMod = member.AttractionModifier(speaker);
                    attrMod = (attrMod > 0) ? attrMod : 0;
                    
                    f = new CharacterRelation()
                    {
                        ID = member.ID,
                        relType = CharacterRelation.Type.Friend
                    };
                    
                    if (listener.IsRival(member) )
                    {
                        effect *= 0.1f;
                    }
                    if (listener.IsFriend(member))
                    {
                        effect *= 4;
                    }
                    
                    r.TryAdd(f, 0);
                    r[f] += effect * attrMod;
                }
            }
            else if (listener.ID == target.ID)
            {
                var attrMod = target.AttractionModifier(speaker);
                attrMod = (attrMod > 0) ? attrMod : 0;
                var f = new CharacterRelation()
                {
                    ID = topic.speaker.ID,
                    relType = CharacterRelation.Type.Friend
                };
                
               r.TryAdd(f, 0);
               r[f] += finalInf * 4 * attrMod;
            }
            else
            {
                var attrFromSpeaker = Mathf.Max(0, listener.AttractionModifier(speaker));
                var attrFromTarget = Mathf.Max(0, listener.AttractionModifier(target));
                
                var currentRelToSpeaker = listener.Data[new CharacterRelation { ID = speaker.ID, relType = CharacterRelation.Type.Friend }];
                var currentRelToTarget = listener.Data[new CharacterRelation { ID = target.ID, relType = CharacterRelation.Type.Friend }];
                
                var effectToSpeaker = finalInf * (currentRelToTarget / 100f) * attrFromSpeaker;
                var effectToTarget = finalInf * (currentRelToSpeaker / 100f) * attrFromTarget;

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
        }

        protected override void OnEnter(Character who)
        {
            base.OnEnter(who);

            who.Busy = true;
            
            Topic defaultTopic = Topics[0];
            defaultTopic.speaker = who;
            
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

            who.Busy = false;
            
            UpdateShare();
        }
        
        protected override void OnDone()
        {
            base.OnDone();
            if (TalkWindow.Instance.EventID == id) TalkWindow.Instance.Close();
        }

        protected override void OnStart()
        {
            scoreBySpeaker.Clear();
            shareOfInfluence.Clear();
            
            UpdateShare();
        }

        public override bool Equals(Event other) => other is TalkEvent;
    }
}