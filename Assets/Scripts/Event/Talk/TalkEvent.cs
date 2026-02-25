using System;
using System.Collections.Generic;
using Game.Object;
using Game.Object.Character;
using UnityEngine;

namespace Game.Event.Talk
{
    [Serializable]
    public class TalkEvent : Event
    {
        [Serializable]
        public class StringTopicDict : UnitySerializedDictionary<string, Topic>
        {
            
        }
        
        [SerializeField] private float minMember = 2;
        [SerializeField] private float maxMember = 8;
        [SerializeField] private float loneliness = 5;
        [SerializeField] private float friendly = 1;
        
        [SerializeField] private float romance = 1;
        [SerializeField] private float skill = 1;
        [SerializeField] private float influence = 1;

        [SerializeField] private bool waitForSelect;
        [SerializeField] private bool alreadySelect;
        
        [SerializeField] public float selectTime = 5f;
        [SerializeField] public float curTime = 0f;

        [SerializeField] public StringTopicDict selected = new();
        
        public TalkEvent()
        {
            eventName = "Talk";
            busy = false;
        }

        public static void StaticInit()
        {
            Topics = new();
            
            Topics.Add(new Topic()
            {
                type = Topic.Type.General
            });
            Topics.Add(new Topic()
            {
                type = Topic.Type.Romance
            });
            
            for (var it = CharacterStatus.SkillBegin + 1; it != CharacterStatus.SkillEnd; it++)
            {
                Topics.Add(new Topic()
                {
                    topic = it,
                    type = Topic.Type.Skill
                });
            }

            for (var it = CharacterStatus.SubjectBegin + 1; it != CharacterStatus.SubjectEnd; it++)
            {
                Topics.Add(new Topic()
                {
                    topic = it,
                    type = Topic.Type.Subject
                });
            }

            foreach (var ch in ObjectManager.Instance.Characters)
            {
                Topics.Add(new Topic()
                {
                    target = ch,
                    type = Topic.Type.RelationUp
                });
                Topics.Add(new Topic()
                {
                    target = ch,
                    type = Topic.Type.RelationDown
                });
            }
        }
        
        private CharacterStatusDeltaFactory Default => new CharacterStatusDeltaFactory(new StatusFloatDict()
        {
            {CharacterStatus.Loneliness, loneliness},
        });

        public static List<Topic> Topics;

        private CharacterStatusDeltaFactory Delta(Character c, Topic topic)
        {
            var ret = Default;
            Character target;
            float value;
            
            //
            // switch (topic.type)
            // {
            //     // case Topic.Type.General:
            //     //     if (c.ID == topic.subject.ID)
            //     //     {
            //     //         foreach (var ch in members)
            //     //         {
            //     //             if (ch.ID == c.ID) continue;
            //     //
            //     //             ret.Add(new CharacterStatus(CharacterStatus.Friendly, ch.ID), friendly);
            //     //         }
            //     //     }
            //     //     else
            //     //     {
            //     //         ret.Add(new CharacterStatus(CharacterStatus.Friendly, topic.subject.ID), friendly);
            //     //     }
            //     //
            //     //     break;
            //     //
            //     // case Topic.Type.Romance:
            //     //     target = topic.subject;
            //     //
            //     //     float attrMod;
            //     //     if (c.ID == topic.subject.ID)
            //     //     {
            //     //         foreach (var ch in members)
            //     //         {
            //     //             attrMod = 2 * target.PersonalAttractionFrom(ch);
            //     //             if (attrMod < 1) continue;
            //     //
            //     //             value = Mathf.Max(romance * attrMod, 1);
            //     //             ret.Add((CharacterStatus)CharacterStatus.RLoneliness, value * value);
            //     //         }
            //     //
            //     //         return ret;
            //     //     }
            //     //
            //     //     attrMod = 2 * c.PersonalAttractionFrom(target);
            //     //     if (attrMod < 1) attrMod = 0;
            //     //
            //     //     value = romance * attrMod;
            //     //
            //     //     ret.Add(new CharacterStatus(CharacterStatus.Romance, target.ID), value);
            //     //
            //     //     break;
            //     // case Topic.Type.Skill:
            //     // case Topic.Type.Subject:
            //     //     var hisSkill = topic.subject.Data[(CharacterStatus)topic.topic];
            //     //     value = (hisSkill / 100f + 1) * skill;
            //     //
            //     //     ret.Add((CharacterStatus)topic.topic, value);
            //     //     ret.Add(new CharacterStatus(CharacterStatus.Friendly, topic.subject.ID), value);
            //     //
            //     //     break;
            //     // case Topic.Type.RelationUp:
            //     //     target = ObjectManager.Instance.Find(topic.target.ID) as Character;
            //     //
            //     //     if (target == null) return new CharacterStatusDeltaFactory();
            //     //
            //     //     var myRelation = c.Data[new CharacterStatus(CharacterStatus.Friendly, topic.target.ID)];
            //     //
            //     //     var meToSub = myRelation / 100f;
            //     //     var meToTarget = influence;
            //     //
            //     //     ret.Add(new CharacterStatus(CharacterStatus.Friendly, topic.subject.ID), meToSub);
            //     //     ret.Add(new CharacterStatus(CharacterStatus.Friendly, topic.target.ID), meToTarget);
            //     //
            //     //     break;
            //     // case Topic.Type.RelationDown:
            //     //     target = ObjectManager.Instance.Find(topic.target.ID) as Character;
            //     //
            //     //     if (target == null) return new CharacterStatusDeltaFactory();
            //     //
            //     //     myRelation = c.Data[new CharacterStatus(CharacterStatus.Friendly, topic.target.ID)];
            //     //
            //     //     meToSub = -myRelation / 100f;
            //     //     meToTarget = -influence;
            //     //
            //     //     ret.Add(new CharacterStatus(CharacterStatus.Friendly, topic.subject.ID), meToSub);
            //     //     ret.Add(new CharacterStatus(CharacterStatus.Friendly, topic.target.ID), meToTarget);
            //     //
            //     //     break;
            //     default:
            //         throw new ArgumentOutOfRangeException();
            // }

            return ret;
        }

        public override CharacterStatusDeltaFactory Delta(Character c)
        {
            CharacterStatusDeltaFactory ret = Default;
            var maxScore = float.NegativeInfinity;
            foreach (var topic in Topics)
            {
                var t = topic;
                t.subject = c;
                    
                var delta = Delta(c, t);
                var score = c.CalcScore(delta, null);

                if (maxScore < score)
                {
                    maxScore = score;
                    ret = delta;
                }
            }

            return ret;
        }

        protected override void OnRun()
        {
            base.OnRun();
            
            if (status == EventStatus.Done) return;

            curTime += UnityEngine.Time.deltaTime;
            if (waitForSelect)
            {
                if (!TalkWindow.Instance.isChoose && curTime < selectTime) return;

                Topic playerTopic = TalkWindow.Instance.Selected;
                playerTopic.subject = GameManager.Instance.Player;

                selected[GameManager.Instance.Player.ID] = playerTopic;

                waitForSelect = false;
                alreadySelect = true;

                return;
            }


            if (!alreadySelect)
            {
                foreach (var ch in members)
                {
                    if (ch.ID == GameManager.Instance.Player.ID)
                    {
                        selected[ch.ID] = TalkWindow.Instance.Selected;
                        continue;
                    }

                    var maxScore = float.NegativeInfinity;
                    Topic maxTopic = Topics[0];
                    maxTopic.subject = ch;
                
                    foreach (var topic in Topics)
                    {
                        var t = topic;
                        t.subject = ch;

                        var delta = Delta(ch, t);
                        var score = ch.CalcScore(delta, null);

                        if (maxScore < score)
                        {
                            maxScore = score;
                            maxTopic = t;
                        }
                    }

                    selected[ch.ID] = maxTopic;
                }
                
                if (members.Contains(GameManager.Instance.Player))
                {
                    waitForSelect = true;
                }

                return;
            }

            if (curTime < selectTime) return;

            foreach (var (cid, topic) in selected)
            {
                var ch = (Character)ObjectManager.Instance.Find(cid);
                var t = topic;
                t.subject = ch;

                foreach (var ch2 in members)
                {
                    var delta = Delta(ch2, t);

                    delta.Apply(ch2, false);
                }
            }

            alreadySelect = false;
            curTime = 0;
        }

        protected override void OnEnter(Character who)
        {
            base.OnEnter(who);
            
            var topic = Topics[0];
            topic.subject = who;
            selected.TryAdd(who.ID, topic);
            
            if (who.ID == GameManager.Instance.Player.ID)
            {
                TalkWindow.Instance.Init(this);
            }
        }

        protected override void OnLeave(Character who)
        {
            base.OnLeave(who);
            
            if (who.ID == GameManager.Instance.Player.ID)
            {
                TalkWindow.Instance.Close();
            }
            
            selected.Remove(who.ID);
        }

        protected override bool CheckRun()
        {
            return members.Count >= minMember && members.Count < maxMember;
        }

        protected override bool CheckInvite(Character who)
        {
            return true;
        }

        protected override void OnDone()
        {
            base.OnDone();
            
            if (TalkWindow.Instance.EventID == id) TalkWindow.Instance.Close();
        }

        public override bool Equals(Event other)
        {
            return other is TalkEvent;
        }
    }
}