using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Event;
using Game.Object;
using Game.Object.Character;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Task
{
    [Serializable]
    public abstract class Effect : IEquatable<Effect>
    {
        public virtual async UniTask DoAsync(CancellationToken token, Character subject, IInteractable other,
            Event.Event e)
        {
            Do(subject, other, e);
            await UniTask.NextFrame();
        }

        public virtual void Do(Character subject, IInteractable other, Event.Event e)
        {
            
        }
        
        public abstract bool Equals(Effect other);

        public virtual void DeltaStats(Character sub, IInteractable obj, ref DeltaResult result)
        {
        }
    }

    [Serializable]
    public class AddDeltaEffect : Effect
    {
        [SerializeField] public CharacterStats deltas;
        [SerializeField] public bool perSec = true;
        [SerializeField] public bool noSideEffect = false;

        public override void Do(Character subject, IInteractable other, Event.Event e)
        {
            DeltaResult result = new DeltaResult()
            {
                Stats = deltas,
                Relation = null
            };
            
            DeltaStats(subject, other, ref result);
            subject.Receive(ref result, perSec, !noSideEffect);
        }

        public override bool Equals(Effect other)
        {
            if (other is not AddDeltaEffect o) return false;

            return deltas.Equals(o.deltas);
        }

        public override void DeltaStats(Character sub, IInteractable obj, ref DeltaResult result)
        {
            result.Stats += deltas;
        }
    }

    public abstract class InviteEventEffect : Effect
    {
        public virtual Event.Event TargetEvent { get; }
        public bool forced = false;
        
        public override async UniTask DoAsync(CancellationToken token,
            Character subject,
            IInteractable other,
            Event.Event e)
        {
            if (token.IsCancellationRequested) return;
            if (other is not Character ch) return;

            TargetEvent.Init();
            
            subject.Busy = true;
            await subject.InviteAsync(token, ch, TargetEvent);
            subject.Busy = false;
        }

        public override void DeltaStats(Character sub, IInteractable obj, ref DeltaResult result)
        {
            if (obj is not Character ch) return;

            if (ch.CurEvent != null)
            {
                ch.CurEvent.CalcDeltaStats(sub, ref result);
                return;
            }

            var e = TargetEvent;

            e.members.Add(sub);
            e.members.Add(ch);

            e.CalcDeltaStats(sub, ref result);
            sub.CalcPersonalizedStatsDeltaOnReceive(ref result);
        }

        public override bool Equals(Effect other)
        {
            if (other is not InviteEventEffect e) return false;

            return e.TargetEvent.Equals(TargetEvent);
        }
    }
    
    [Serializable]
    public class InviteSimpleEventEffect : InviteEventEffect
    {
        public override Event.Event TargetEvent => EventManager.Instance.CreateSimpleEvent(eventData);
        [SerializeField] public EventBase eventData;
        
        public override bool Equals(Effect other)
        {
            if (other is not InviteSimpleEventEffect o) return false;

            return eventData == o.eventData;
        }
    }

    [Serializable]
    public class TractTargetEffect : Effect
    {
        [SerializeReference] public string targetID;
        
        public override async UniTask DoAsync(CancellationToken token, Character subject, IInteractable other,
            Event.Event e)
        {
            if (token.IsCancellationRequested) return;
            
            await subject.TrackTargetAsync(token, targetID);
        }

        public override bool Equals(Effect other)
        {
            if (other is not TractTargetEffect o) return false;

            return targetID == o.targetID;
        }
    }

    public class WalkToEffect : Effect
    {
        [SerializeField] public Vector3Int cpos;
        
        public override async UniTask DoAsync(CancellationToken token, Character subject, IInteractable other,
            Event.Event e)
        {
            if (token.IsCancellationRequested) return;
            
            await subject.WalkAsync(token, new []{cpos});
        }

        public override bool Equals(Effect other)
        {
            if (other is not WalkToEffect o) return false;

            return cpos == o.cpos;
        }
    }
    
    [Serializable]
    public class JumpEffect : Effect
    {
        [SerializeField] public Vector3Int position;
        public override void Do(Character subject, IInteractable other, Event.Event e)
        {
            subject.Position = position;
        }
        
        public override bool Equals(Effect other)
        {
            if (other is not JumpEffect o) return false;

            return position == o.position;
        }
    }

    public enum VarType
    {
        Character,
        Task,
        Event,
        Global
    }

    [Serializable]
    public class SetVar : Effect
    {
        [SerializeField] public VarType vType;
        [SerializeField] public string name;
        [SerializeField] public float val;

        public override void Do(Character subject, IInteractable other, Event.Event e)
        {
            switch (vType)
            {
                case VarType.Character:
                    subject.SetVar(name, val);
                    break;
                case VarType.Task:
                    subject.TaskQueue.Front.SetVar(name, val);
                    break;
                case VarType.Event:
                    e.SetVar(name, val);
                    break;
                case VarType.Global:
                    GameManager.Instance.SetVar(name, val);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override bool Equals(Effect other)
        {
            if (other is not SetVar t) return false;
            return (t.name == name && t.vType == vType && Mathf.Approximately(t.val, val));
        }
    }

    [Serializable]
    public class AddVar : Effect
    {
        [SerializeField] public VarType vType;
        [SerializeField] public string name;
        [SerializeField] public float val;
        [SerializeField] public bool perSec;

        public override void Do(Character subject, IInteractable other, Event.Event e)
        {
            var v = val;
            if (perSec) v = val * UnityEngine.Time.deltaTime;

            float x;
            switch (vType)
            {
                case VarType.Character:
                    x = subject.GetVar(name);
                    subject.SetVar(name, x + v);
                    break;
                case VarType.Task:
                    x = subject.TaskQueue.Front.GetVar(name);
                    subject.TaskQueue.Front.SetVar(name, x + v);
                    break;
                case VarType.Event:
                    x = e.GetVar(name);
                    e.SetVar(name, x+v);
                    break;
                case VarType.Global:
                    x = GameManager.Instance.GetVar(name);
                    GameManager.Instance.SetVar(name, x + v);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override bool Equals(Effect other)
        {
            if (other is not AddVar t) return false;
            return (t.name == name && t.vType == vType && Mathf.Approximately(t.val, val) && t.perSec == perSec);
        }
    }
    
    [Serializable]
    public class ListEffect : Effect
    {
        [SerializeReference] public List<Effect> list = new();
        
        public override async UniTask DoAsync(CancellationToken token, Character subject, IInteractable other,
            Event.Event e)
        {
            if (token.IsCancellationRequested) return;

            foreach (var effect in list)
            {
                if (token.IsCancellationRequested) return;
                await effect.DoAsync(token, subject, other, e);
            }
        }

        public override bool Equals(Effect other)
        {
            if (other is not ListEffect e) return false;
            if (list.Count != e.list.Count) return false;
            
            for(int i = 0;i < list.Count;i++)
            {
                if (!list[i].Equals(e.list[i])) return false;
            }

            return true;
        }
    }

    public class ActionEffect : Effect
    {
        public UnityAction Action;

        public override void Do(Character subject, IInteractable other, Event.Event e)
        {
            Action?.Invoke();
        }
        
        public override bool Equals(Effect other)
        {
            if (other is not ActionEffect t) return false;
            return Action == t.Action;
        }
    }
}