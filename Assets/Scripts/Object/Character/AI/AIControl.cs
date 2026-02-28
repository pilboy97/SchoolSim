using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Debug;
using Game.Event;
using Game.Task;
using Unity.Mathematics;
using UnityEngine;
using Action = Game.Task.Action;

namespace Game.Object.Character.AI
{
    [Serializable]
    public class AIControl : IController
    {
        [SerializeField] private Character character;

        public AIControl(Character character)
        {
            this.character = character;
        }

        public Character Character => character;
        private const int RandomSize = 6;
        private readonly (IInteractable obj, Action action, float score)[] _topActions = new (IInteractable obj, Action action, float score)[RandomSize];

        public ITask Select()
        {
            var objs = ObjectManager.Instance.Objects;
            var currentTask = character.TaskQueue.Current;

            for (var i = 0; i < RandomSize; i++)
            {
                _topActions[i].score = -1;
            }
                
            for (int i = 0; i < objs.Count; i++)
            {
                var o = objs[i];
                if (o?.Actions == null) continue;
                
                for (int j = 0; j < o.Actions.Length; j++)
                {
                    var action = o.Actions[j];
                    if (o.ID == character.ID && !action.allowSelf) continue;
                    if (!action.Check(character, o)) continue;

                    var score = CalcScore(action.DeltaStats(character, o), o);

                    // 관성 보너스
                    if (currentTask is ActionTask actionTask &&
                        actionTask.action.Equals(action) &&
                        actionTask.Obj.Equals(o))
                    {
                        score *= 1.2f;
                    }
                    else if (currentTask is EventTask eventTask && 
                        action.effect is InviteEventEffect invite && 
                        eventTask.invitedEvent.Equals(invite.TargetEvent))
                    {
                        score *= 1.2f;
                    }

                    if (score <= 0) continue;

                    // 상위 N개 유지
                    InsertTopManual((o, action, score));
                }
            }

            // 룰렛 선택을 위해 점수만 따로 추출 (LINQ Select 대신 반복문 사용)
            var scoresOnly = new List<float>(RandomSize);
            for (int i = 0; i < RandomSize; i++)
            {
                if (_topActions[i].score > 0)
                    scoresOnly.Add(_topActions[i].score);
                else break;
            }

            int idx = Random.SelectWithMultiplier(scoresOnly);
            if (idx < 0 || idx > RandomSize) return null;
             
            var selected = _topActions[idx];

            // 결정 임계치 체크
            float idleScore = CalcScore();
            if (selected.score < idleScore * 1.3f) return null;

            // 현재 행동과 동일한지 체크
            if (currentTask is ActionTask a &&
                a.action.Equals(selected.action) &&
                a.Obj.Equals(selected.obj))
            {
                return null;
            }
            else if (currentTask is EventTask e && 
                     selected.action.effect is InviteEventEffect invite && 
                     e.invitedEvent.Equals(invite.TargetEvent))
            {
                return null;
            }

            return new ActionTask(character, selected.obj, selected.action);
        }

        // 정렬 기능을 직접 구현하여 LINQ 의존성 제거
        private void InsertTopManual((IInteractable, Action, float) item)
        {
            if (_topActions[RandomSize - 1].score >  item.Item3) return;

            _topActions[RandomSize - 1] = item;

            // 삽입 정렬 방식으로 내림차순 정렬 (데이터가 작으므로 매우 빠름)
            for (int i = RandomSize - 1; i > 0; i--)
            {
                if (_topActions[i].score > _topActions[i - 1].score)
                {
                    (_topActions[i], _topActions[i - 1]) = (_topActions[i - 1], _topActions[i]);
                }
                else break;
            }
        }

        public async UniTask<bool> TryInviteMeAsync(CancellationToken token, Event.Event e, Character who,
            bool forced = false)
        {
            if (token.IsCancellationRequested) return false;
            if (character.Data.eventID != "") return false;

            if (!forced)
            {
                var x = CalcScore();
                var y = CalcScore(e.CalcDeltaStats(character),null) ; 

                if (TryInterrupt() && x > y)
                {
                    var front = character.TaskQueue.Front;

                    if (front is not ActionTask actionTask ||
                        actionTask.action.effect is not InviteSimpleEventEffect frontEffect) return false;
                    if (!e.Equals(frontEffect.eventData)) return false;

                    var ch = actionTask.Obj as Character;

                    if (ch?.CurEvent?.members != null && !ch.CurEvent.members.Contains(who)) return false;
                }
            }

            if (!e.TryInvite(character, forced))
            {
                return false;
            }

            return true;
        }

        private void Log(SelectLogData data)
        {
            DebugSystem.Instance.Datas.Add(data);
        }

        public bool TryInterrupt()
        {
            return !character.Busy;
        }
        
        private CharacterStats CalcENeedScoreMultiplier(CharacterStats val)
        {
            return new CharacterStats()
            {
                hungry = CalcENeedScoreMultiplier(val.hungry),
                fatigue = CalcENeedScoreMultiplier(val.fatigue),
                hygiene = CalcENeedScoreMultiplier(val.hygiene),
                toilet = CalcENeedScoreMultiplier(val.toilet),
            };
        }
        private float CalcENeedScoreMultiplier(float val)
        {
            return Mathf.Pow(0.5f, (val - 64f) / 3);
        }

        private CharacterStats CalcRNeedScoreMultiplier(CharacterStats val)
        {
            return new CharacterStats()
            {
                fun = CalcRNeedScoreMultiplier(val.fun),
                loneliness = CalcRNeedScoreMultiplier(val.loneliness),
                rLoneliness = CalcRNeedScoreMultiplier(val.rLoneliness),
            };
        }
        private float CalcRNeedScoreMultiplier(float val)
        {
            return Mathf.Max(0, 1.2f * (100f - val));
        }

        private CharacterStats CalcGNeedScoreMultiplier(CharacterStats val)
        {
            return new CharacterStats()
            {
                motivation = CalcGNeedScoreMultiplier(val.motivation)
            };
        }

        private float CalcGNeedScoreMultiplier(float val)
        {
            // 1. 기본적으로는 숙련도가 낮을 때 아주 최소한의 '배워야겠다'는 의지가 있음 (15점)
            float baseWill = 10f; 
            
            float ratio = val / 100f;
            float passion = (ratio * ratio) * 100;

            return baseWill + passion;
        }

        public float CalcScore()
        {
            if (character.taskQueue.Current != null) return CalcScore(character.taskQueue.Current.CalcDeltaForScore(), null);

            return 0;
        }

        public float CalcScore(RelationFloatDict deltas, IInteractable o)
        {
            var statsDelta = new CharacterStats();
            
            foreach (var (rel, v) in deltas)
            {
                statsDelta += character.CalcPersonalizedStatsDeltaOnReceive(rel, v);
            }

            return CalcScore(statsDelta, o);
        }

        public float CalcScore((CharacterStats, RelationFloatDict ) deltaStats, IInteractable o)
        {
            return CalcScore(deltaStats.Item1, o) + CalcScore(deltaStats.Item2 ?? new(), o);
        }
        
        public float CalcScore(CharacterStats deltas, IInteractable o)
        {
            var score =
                (deltas *
                 (CalcENeedScoreMultiplier(character.Data.stats) * character.Data.eModifier +
                  CalcRNeedScoreMultiplier(character.Data.stats) * character.Data.rModifier +
                  CalcGNeedScoreMultiplier(character.Data.stats) * character.Data.gModifier)).SumNeeds();
            
            var dist = NavManager.Instance.FindPathAround(character.Position, o?.Positions ?? new Vector3[] {
                character.Position
            }).Item2;

            return score / math.max(dist, 1f);
        }
        
        
        public float CalcScore(ActionTask task)
        {
            CharacterStats s = new();
            RelationFloatDict r = null;
            
            if (task.action.effect is AddDeltaEffect addDeltaEffect)
                (s,r) = addDeltaEffect.DeltaStats(task.Sub, task.Obj);
            else if (task.action.effect is InviteSimpleEventEffect inviteEventEffect)
                (s,r) = inviteEventEffect.DeltaStats(task.Sub, task.Obj);
            return CalcScore((s,r), task.Obj);
        }
        
        protected void CalcDeltaStats(Character c, CharacterStats delta, RelationFloatDict relDelta)
        {
            c.CalcPersonalizedStatsDeltaOnReceive(relDelta);
            c.CalcPersonalizedStatsDeltaOnReceive(delta);
        }
    }
}