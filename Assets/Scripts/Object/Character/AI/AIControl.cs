using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
// using System.Linq;
using Game.Debug;
using Game.Event.Talk;
using Game.Task;
using Game.Time;
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

            IInteractable obj = null;
            Action selected = new Action();
            float maxScore = -1;

            foreach (var o in objs)
            {
                if (o?.Actions == null) continue;

                foreach (var action in o.Actions)
                {
                    if (o.ID == character.ID && !action.allowSelf) continue;
                    if (!action.Check(character, o)) continue;

                    var score = character.CalcScore(action.Delta(character, o), o);

                    if (score <= 0) continue;
                    
                    if (score <= maxScore) continue;
                    
                    maxScore = score;
                    obj = o;
                    selected = action;
                }
            }

            if (maxScore < 0 || obj == null) return null;
            if (maxScore <= character.CalcScore() * 2f) return null;

            var ret = new ActionTask(character, obj, selected);
            var current = character.TaskQueue.Current;

            if (current is EventTask) return ret;
            else
            {
                if (current is ActionTask actionTask && actionTask.action.Equals(selected)) return null;
            }


            return ret;
        }

        public async UniTask<bool> TryInviteMeAsync(CancellationToken token, Event.Event e, Character who,
            bool forced = false)
        {
            if (token.IsCancellationRequested) return false;
            if (character.Data.eventID != "") return false;
            
            if (!forced)
            {
                var x = character.CalcScore();
                var y = character.CalcScore(e.Delta(character), who);

                if (TryInterrupt() && x > y)
                {
                    var front = character.TaskQueue.Front;
                    
                    if (front is not ActionTask actionTask || actionTask.action.effect is not InviteSimpleEventEffect frontEffect) return false;
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
    }
}