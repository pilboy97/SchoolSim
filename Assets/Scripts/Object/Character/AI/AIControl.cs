using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Task;
using Sirenix.OdinInspector;
using UnityEngine;
using Action = Game.Task.Action;

namespace Game.Object.Character.AI
{
    [Serializable]
    public class AIControl : IController
    {
        [SerializeField] private Character character;
        [ShowInInspector] private float inertia => ConfigData.Instance.inertia;

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
                    var score = CalcScore(o, _result);

                    // 관성 보너스
                    if (currentTask is ActionTask actionTask &&
                        actionTask.action.Equals(action) &&
                        actionTask.Obj.Equals(o))
                    {
                        score *= inertia;
                    }
                    else if (currentTask is EventTask eventTask && 
                             action.effect is InviteEventEffect invite && 
                             eventTask.invitedEvent.Equals(invite.TargetEvent))
                    {
                        score *= inertia;
                    }

                    if (score <= 0) continue;

                    // 상위 N개 유지
                    InsertTopManual((o, action, score));
                }
            }
            
            var scoresOnly = new List<float>(RandomSize);
            for (int i = 0; i < RandomSize; i++)
            {
                if (_topActions[i].score > 0)
                {
                    scoresOnly.Add(_topActions[i].score);
                }
                else break;
            }

            int idx = Random.SelectWithMultiplier(scoresOnly);
            if (idx < 0 || idx > RandomSize) return null;

            _result.Reset();
            
            var selected = _topActions[idx];
            
            // 결정 임계치 체크
            float curScore = CalcScore(ref _result);
            if (selected.score < curScore * 1.3f) return null;

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
            var y = CalcScore(null, _result) ; 

            if (TryInterrupt() && x * inertia > y)
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
        
        private float CalcENeedScoreMultiplier(float val)
        {
            var diff = (100 - val > 0) ? 100 - val : 0;
            return Mathf.Pow(0.3f, -(diff - 50) / 3);
        }
        private float CalcRNeedScoreMultiplier(float val)
        {
            var diff = (100 - val > 0) ? 100 - val : 0;
            return diff * diff * diff;
        }

        private float CalcGNeedScoreMultiplier(float val)
        {
            var diff = (100 - val > 0) ? 100 - val : 0;
            return 2000f * (diff) + 1000;
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

        private CharacterStats CalcRNeedScoreMultiplier(CharacterStats val)
        {
            return new CharacterStats()
            {
                fun = CalcRNeedScoreMultiplier(val.fun),
                loneliness = CalcRNeedScoreMultiplier(val.loneliness),
                rLoneliness = CalcRNeedScoreMultiplier(val.rLoneliness),
            };
        }

        private CharacterStats CalcGNeedScoreMultiplier(CharacterStats val)
        {
            return new CharacterStats()
            {
                motivation = CalcGNeedScoreMultiplier(val.motivation)
            };
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
        
        public float CalcScore(IInteractable o, DeltaResult result)
        {
            // 1. 관계(Relation) 변화로 인해 얻는 스탯(외로움 해소 등)을 result.Stats에 누적합니다.
            if (result.Relation != null)
            {
                foreach (var (rel, v) in result.Relation)
                {
                    character.CalcPersonalizedStatsDeltaOnReceive(rel, v, ref result); // _result가 아님!
                }
            }
            // 2. 최종적으로 합산된 스탯을 바탕으로 점수를 계산합니다.
            return CalcScore(result.Stats, o, ref result);
        }
        
        public float CalcScore(RelationFloatDict deltas, IInteractable o, ref DeltaResult result)
        {
            if (deltas == null) return 0;
            
            foreach (var (rel, v) in deltas)
            {
                character.CalcPersonalizedStatsDeltaOnReceive(rel, v, ref result);
            }

            return CalcScore(result.Stats, o, ref result);
        }

        
        public float CalcScore(CharacterStats deltas, IInteractable o, ref DeltaResult result)
        {
            // 기본 모디파이어
            float eMod = ConfigData.Instance.eModifier;
            float rMod = ConfigData.Instance.rModifier;
            float gMod = ConfigData.Instance.gModifier;

            var i_e_mod = ConfigData.Instance.I_E_modifier;
            var n_s_mod = ConfigData.Instance.N_S_modifier;

            if (character.Data.mbti.CheckComponent(MBTIComponent.S))
            {
                eMod *= n_s_mod; 
            }
            else if (character.Data.mbti.CheckComponent(MBTIComponent.N))
            {
                gMod *= n_s_mod;
            }

            if (character.Data.mbti.CheckComponent(MBTIComponent.E))
            {
                rMod *= i_e_mod;
            }
            else
            {
                rMod /= i_e_mod;
            }
            
            var score =
                (deltas *
                 (CalcENeedScoreMultiplier(character.Data.stats) * eMod +
                  CalcRNeedScoreMultiplier(character.Data.stats) * rMod +
                  CalcGNeedScoreMultiplier(character.Data.stats) * gMod)).SumNeeds();
            
            var dist = NavManager.Instance.FindPathAround(character.Position, o?.Positions).Item2;
            float distancePenalty = 1f + (dist * 0.1f); 

            return score / distancePenalty;
        }


        public float CalcScore(ActionTask task)
        {
            _result.Reset();
            
            if (task.action.effect is AddDeltaEffect addDeltaEffect)
                addDeltaEffect.DeltaStats(task.Sub, task.Obj, ref _result);
            else if (task.action.effect is InviteSimpleEventEffect inviteEventEffect)
                inviteEventEffect.DeltaStats(task.Sub, task.Obj, ref _result);
            return CalcScore(task.Obj, _result);
        }
        
        protected void CalcDeltaStats(Character c, ref DeltaResult result)
        {
            c.CalcPersonalizedStatsDeltaOnReceive(ref result);
        }
    }
}