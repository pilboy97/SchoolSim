using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Event;
using Game.Object;
using Game.Object.Character;
using UnityEngine;

namespace Game.Task
{
    public class ActionTask : ITask
    {
        private Character sub;
        private IInteractable obj;

        private bool busy;

        public Action action;
        private string id;

        public string ID => id;

        public bool Busy
        {
            get => busy;
            set => busy = value;
        }

        public Character Sub => sub;
        public IInteractable Obj => obj;

        public ActionTask(Character sub, IInteractable obj, Action action, string id = null)
        {
            this.sub = sub;
            this.obj = obj;
            this.action = action;

            busy = action.busy;

            this.id = id ?? IHasID.GenerateID();
        }

        public string Desc => action.actionName;
        public ITask Prev { get; set; }

        public async UniTask DoAsync(CancellationToken token)
        {
            if (Prev != null) await Prev.DoAsync(token);

            if (action.effect == null) return;
            if (!action.indirect && (obj?.Distance(sub) ?? 0) >= 1.2f) return;

            while (!token.IsCancellationRequested)
            {
                await action.effect.DoAsync(token, sub, obj, null);

                if (!action.notOnce) break;
            }
            
            GameManager.Instance.ClearValue(ID);
        }

        public float CalcScore()
        {
            CharacterStatusDeltaFactory delta = new();
            if (action.effect is AddDeltaEffect addDeltaEffect)
                delta = addDeltaEffect.Delta(sub, obj);
            else if (action.effect is InviteSimpleEventEffect inviteEventEffect)
                delta = inviteEventEffect.Delta(sub, obj);
            return sub.CalcScore(delta, obj);
        }
    }
}