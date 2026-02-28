using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Event;
using Game.Object;
using Game.Object.Character;
using UnityEngine;

namespace Game.Task
{
    public class EventTask : ITask
    {
        private Character sub;
        [SerializeReference] public Event.Event invitedEvent;
        private string id;
        private bool busy;

        public bool Busy
        {
            get => busy;
            set => busy = value;
        }

        public EventTask(Event.Event invitedEvent, Character sub, string id = null)
        {
            this.invitedEvent = invitedEvent;

            this.sub = sub;
            this.id = id ?? IHasID.GenerateID();

            busy = invitedEvent.busy;
        }

        public string Desc => invitedEvent.eventName;
        public ITask Prev { get; set; }

        public Character Sub => sub;

        public async UniTask DoAsync(CancellationToken token)
        {
            if (token.IsCancellationRequested) return;

            if (Prev != null) await Prev.DoAsync(token);

            while (invitedEvent.Status != EventStatus.Done)
            {
                if (token.IsCancellationRequested)
                    break;

                await UniTask.NextFrame();
            }
            
            invitedEvent.Leave(sub);
            GameManager.Instance.ClearValue(ID);
        }

        public CharacterStats CalcDeltaForScore()
        {
            var (s, r) = invitedEvent.CalcDeltaStats(sub);
            return s + sub.CalcPersonalizedStatsDeltaOnReceive(r);
        }

        public string ID => id;
    }
}