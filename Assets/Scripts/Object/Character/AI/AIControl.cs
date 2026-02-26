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

        public ITask Select()
        {
            var objs = ObjectManager.Instance.Objects;
            var currentTask = character.TaskQueue.Current as ActionTask;

            // 상위 3개 후보를 담을 리스트 (LINQ 없이 관리)
            var topActions = new List<(IInteractable obj, Action action, float score)>();

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

                    // 관성(Hysteresis) 보너스
                    if (currentTask != null && currentTask.action.Equals(action) && currentTask.Obj.Equals(o))
                    {
                        score *= 1.2f;
                    }

                    if (score <= 0) continue;

                    // 상위 N개 유지 (직접 구현한 헬퍼 함수 사용)
                    InsertTopManual(topActions, (o, action, score), 3);
                }
            }

            if (topActions.Count == 0) return null;

            // 룰렛 선택을 위해 점수만 따로 추출 (LINQ Select 대신 반복문 사용)
            var scoresOnly = new List<float>(topActions.Count);
            for (int i = 0; i < topActions.Count; i++)
            {
                scoresOnly.Add(topActions[i].score);
            }

            int idx = Random.SelectWithMultiplier(scoresOnly);
            var selected = topActions[idx];

            // 결정 임계치 체크
            float idleScore = CalcScore();
            if (selected.score < idleScore * 1.1f) return null;

            // 현재 행동과 동일한지 체크
            if (currentTask != null && currentTask.action.Equals(selected.action) &&
                currentTask.Obj.Equals(selected.obj))
            {
                return null;
            }

            return new ActionTask(character, selected.obj, selected.action);
        }

        // 정렬 기능을 직접 구현하여 LINQ 의존성 제거
        private void InsertTopManual(List<(IInteractable obj, Action action, float score)> list,
            (IInteractable, Action, float) item, int maxCount)
        {
            list.Add(item);

            // 삽입 정렬 방식으로 내림차순 정렬 (데이터가 작으므로 매우 빠름)
            for (int i = list.Count - 1; i > 0; i--)
            {
                if (list[i].score > list[i - 1].score)
                {
                    (list[i], list[i - 1]) = (list[i - 1], list[i]);
                }
                else break;
            }

            if (list.Count > maxCount)
            {
                list.RemoveAt(maxCount);
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
                var y = CalcScore(e.CalcDeltaStats(character), who);

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
        private float CalcENeedScoreMultiplier(float val)
        {
            return Mathf.Pow(0.5f, (val - 64f) / 3);
        }

        private float CalcRNeedScoreMultiplier(float val)
        {
            return Mathf.Max(0, 1.2f * (100f - val));
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
            var statsDelta = new CharacterStatusDeltaFactory();
            
            foreach (var (rel, v) in deltas)
            {
                var effect = character.CalcPersonalizedStatsDeltaOnReceiveStatsDelta(rel, v);
                statsDelta.Add(effect);
            }

            return CalcScore(statsDelta, o);
        }

        public float CalcScore((CharacterStatusDeltaFactory, RelationFloatDict ) deltaStats, IInteractable o)
        {
            return CalcScore(deltaStats.Item1 ?? new (), o) + CalcScore(deltaStats.Item2 ?? new(), o);
        }
        
        public float CalcScore(CharacterStatusDeltaFactory deltas, IInteractable o)
        {
            var eNeedScore = 0f;
            for (var need = CharacterStatus.ENeedsBegin; need < CharacterStatus.ENeedsEnd; need++)
            {
                eNeedScore += CalcENeedScoreMultiplier(
                                  character.Data[need]
                              ) *
                              deltas[need];
            }

            var rNeedScore = 0f;
            for (var need = CharacterStatus.RNeedsBegin; need < CharacterStatus.RNeedsEnd; need++)
            {
                rNeedScore += CalcRNeedScoreMultiplier(
                                  character.Data[need]
                              ) *
                              deltas[need];
            }

            var gNeedScore = 0f;
            for (var need = CharacterStatus.GNeedsBegin; need < CharacterStatus.GNeedsEnd; need++)
            {
                gNeedScore += CalcGNeedScoreMultiplier(
                                  character.Data[need]
                              ) *
                              deltas[need];
            }

            var score = eNeedScore * character.Data.eModifier +
                        rNeedScore * character.Data.rModifier +
                        gNeedScore * character.Data.gModifier;
            
            var dist = NavManager.Instance.FindPathAround(character.Position, o?.Positions ?? new Vector3[] {
                character.Position
            }).Item2;

            return score / math.max(dist, 1f);
        }
        
        
        public float CalcScore(ActionTask task)
        {
            CharacterStatusDeltaFactory delta = new();
            if (task.action.effect is AddDeltaEffect addDeltaEffect)
                delta = addDeltaEffect.DeltaStats(task.Sub, task.Obj);
            else if (task.action.effect is InviteSimpleEventEffect inviteEventEffect)
                delta = inviteEventEffect.DeltaStats(task.Sub, task.Obj);
            return CalcScore(delta, task.Obj);
        }
        
        protected (CharacterStatusDeltaFactory, RelationFloatDict) CalcDeltaStats(Character c,CharacterStatusDeltaFactory delta, RelationFloatDict relDelta)
        {
            var effect =c. CalcPersonalizedStatsDeltaOnReceiveStatsDelta(relDelta);
            return (c.CalcPersonalizedStatsDeltaOnReceiveStatsDelta(delta + effect), relDelta);
        }
    }
}