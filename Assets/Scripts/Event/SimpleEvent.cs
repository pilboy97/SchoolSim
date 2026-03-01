using System;
using Game.Object.Character;
using Unity.Android.Types;
using Action = Game.Task.Action;

namespace Game.Event
{
    public enum EventStatus
    {
        Ready,
        Run,
        Done
    }
    
    [Serializable]
    public class SimpleEvent : Event
    {
        public EventBase data;
        
        public SimpleEvent(EventBase e)
        {
            data = e;
            status = EventStatus.Ready;
            eventName = e.eventName;
            zombie = data.zombie;
            busy = data.busy;
        }

        public override void CalcDeltaStats(Character c, ref DeltaResult result)
        {
            if (c == null) return;

            data.effect.DeltaStats(c, null, ref result);
            
            c.CalcPersonalizedStatsDeltaOnReceive(ref result);
        }

        protected override void OnStart()
        {
            data.onStart?.Do(null, null, this);
        }

        protected override void OnEnter(Character who)
        {
            data.onEnter?.Do(who, null, this);
        }

        protected override void OnLeave(Character who)
        {
            data.onLeave?.Do(who, null, this);
        }

        private DeltaResult _result;
        
        protected override void OnRun()
        {
            base.OnRun();
            if (status != EventStatus.Run) return;


            foreach (var ch in members)
            {
                _result.Reset();
                CalcDeltaStats(ch, ref _result);

                ch.Receive(ref _result);
            }
        }

        protected override void OnDone()
        {
            data.onEnd?.Do(null, null, this);
        }

        protected override bool CheckRun()
        {
            return data.runCond.Check(this, new Action(), null, null);
        }

        protected override bool CheckInvite(Character who)
        {
            return data.runCond.Check(this, new Action(), who, null);
        }

        public override bool Equals(Event other)
        {
            if (other is not SimpleEvent simp) return false;

            return data == simp.data;
        }

        public override bool Equals(EventBase data)
        {
            return this.data == data;
        }
    }
}