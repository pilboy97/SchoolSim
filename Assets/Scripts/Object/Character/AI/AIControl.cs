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
        
        private DeltaResult _result = new DeltaResult()
        {
            Stats = default,
            Relation = new RelationFloatDict()
        };
        
        public ITask Select()
        {
            var objs = ObjectManager.Instance.Objects;
            var currentTask = character.TaskQueue.Current;

            for (var i = 0; i < RandomSize; i++)
            {
                _topActions[i].score = -1;
            }
                
            foreach (var o in objs)
            {
                if (o?.Actions == null) continue;

                foreach (var action in o.Actions)
                {
                    if (o.ID == character.ID && !action.allowSelf) continue;
                    if (!action.Check(character, o)) continue;

                    _result.Reset();

                    action.DeltaStats(character, o, ref _result);
                    var score = CalcScore(o, ref _result);

                    // 관성 보너스
                    if (currentTask is ActionTask actionTask &&
                        actionTask.action.Equals(action) &&
                        actionTask.Obj.Equals(o))
                    {
                        score *= 5f;
                    }
                    else if (currentTask is EventTask eventTask && 
                             action.effect is InviteEventEffect invite && 
                             eventTask.invitedEvent.Equals(invite.TargetEvent))
                    {
                        score *= 5f;
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

            _result.Reset();
            
            var selected = _topActions[idx];

            // 결정 임계치 체크
            float idleScore = CalcScore(ref _result);
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

        public UniTask<bool> TryInviteMeAsync(CancellationToken token, Event.Event e, Character who,
            bool forced = false)
        {
            if (token.IsCancellationRequested) return  UniTask.FromResult(false);
            if (character.Data.eventID != "")return  UniTask.FromResult(false);

            if (forced) return UniTask.FromResult(e.TryInvite(character, true));
            
            _result.Reset();
            
            var x = CalcScore(ref _result);

            e.CalcDeltaStats(character, ref _result);
            
            _result.Reset();
            var y = CalcScore(null, ref _result) ; 

            if (TryInterrupt() && x > y)
            {
                var front = character.TaskQueue.Front;

                if (front is not ActionTask actionTask ||
                    actionTask.action.effect is not InviteSimpleEventEffect frontEffect) return UniTask.FromResult(false);
                if (!e.Equals(frontEffect.eventData)) return UniTask.FromResult(false);

                var ch = actionTask.Obj as Character;

                if (ch?.CurEvent?.members != null && !ch.CurEvent.members.Contains(who)) return UniTask.FromResult(false);
            }

            return UniTask.FromResult(e.TryInvite(character));
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
            return Mathf.Max(0, 2f * (100f - val));
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
            float baseWill = 10f; 
            
            float ratio = val / 100f;
            float passion = (ratio * ratio * ratio) * 100;

            return baseWill + passion;
        }

        public float CalcScore(ref DeltaResult result)
        {
            result.Reset();

            if (character.taskQueue.Current != null)
            {
                character.taskQueue.Current.CalcDeltaForScore(ref result);
                return CalcScore(result.Relation, null, ref result) + CalcScore(result.Stats, null, ref result);
            }

            return 0;
        }

        public float CalcScore(IInteractable o, ref DeltaResult result)
        {
            return CalcScore(result.Relation, o, ref result) + CalcScore(result.Stats, o, ref result);
        }
        
        public float CalcScore(RelationFloatDict deltas, IInteractable o, ref DeltaResult result)
        {
            if (deltas == null) return 0;
            
            foreach (var (rel, v) in deltas)
            {
                character.CalcPersonalizedStatsDeltaOnReceive(rel, v, ref _result);
            }

            return CalcScore(result.Stats, o, ref result);
        }

        
        public float CalcScore(CharacterStats deltas, IInteractable o, ref DeltaResult result)
        {
            var score =
                (deltas *
                 (CalcENeedScoreMultiplier(character.Data.stats) * character.Data.eModifier +
                  CalcRNeedScoreMultiplier(character.Data.stats) * character.Data.rModifier +
                  CalcGNeedScoreMultiplier(character.Data.stats) * character.Data.gModifier)).SumNeeds();
            
            var dist = NavManager.Instance.FindPathAround(character.Position, o?.Positions).Item2;

            return score / math.max(dist, 1f);
        }


        public float CalcScore(ActionTask task)
        {
            _result.Reset();
            
            if (task.action.effect is AddDeltaEffect addDeltaEffect)
                addDeltaEffect.DeltaStats(task.Sub, task.Obj, ref _result);
            else if (task.action.effect is InviteSimpleEventEffect inviteEventEffect)
                inviteEventEffect.DeltaStats(task.Sub, task.Obj, ref _result);
            return CalcScore(task.Obj, ref _result);
        }
        
        protected void CalcDeltaStats(Character c, ref DeltaResult result)
        {
            c.CalcPersonalizedStatsDeltaOnReceive(ref result);
        }
    }
}