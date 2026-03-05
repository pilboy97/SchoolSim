using System;
using System.Collections.Generic;
using Game.Object.Character;
using UnityEngine;
using EventTask = Game.Task.EventTask;

namespace Game.Event
{
    [Serializable]
    public abstract class Event : IEquatable<Event>
    {
        // --- 설정 필드 ---
        [Header("Settings")]
        [SerializeField]
        public float minMember = 2;
        [SerializeField] public float maxMember = 8;
        
        [SerializeField] protected string id = "";
        [SerializeField] protected EventStatus status = EventStatus.Ready;
        public EventStatus Status => status; 
        public List<Character> members = new();
        public bool zombie;
        public string eventName;
        public bool busy;
        public void Init()
        {
            if (id == "")
            {
                id = IHasID.GenerateID();
            }
            
            members.Clear();
        }
        
        public virtual void Update()
        {
            switch (Status)
            {
                case EventStatus.Ready:
                    OnReady();
                    
                    break;
                case EventStatus.Done:
                    OnDone();
                    
                    break;
                case EventStatus.Run:
                    OnRun();

                    break;
            }
        }

        protected virtual void OnReady()
        {
            if (!CheckRun()) return;
            
            status = EventStatus.Run;
            
            OnStart();
        }

        public string EventDesc => $"{eventName}:{ID}";
        protected virtual void OnStart()
        {
        }

        protected virtual void OnEnter(Character who)
        {
        }

        protected virtual void OnLeave(Character who)
        {
            if (who.taskQueue.Current is EventTask) who.taskQueue.Cancel();
            who.OnLeaveEvent();
        }
        
        protected virtual void OnRun()
        {
            if (CheckRun()) return;
            
            status = EventStatus.Done;
        }
        protected virtual void OnDone()
        {
        }

        public abstract void CalcDeltaStats(Character c, ref DeltaResult result);
        

        protected virtual bool CheckRun() => members.Count >= minMember;
        protected virtual bool CheckInvite(Character who) => members.Count + 1 <= maxMember;


        public string ID => id;

        public bool TryInvite(Character who, bool forced = false)
        {
            if (status == EventStatus.Done) return false;
            if (members.Contains(who)) return false;
            if (!forced)
            {
                if (!CheckInvite(who)) return false;
            }
            
            members.Add(who);

            OnEnter(who);
            return true;
        }

        public void Leave(Character who)
        {
            members.Remove(who);
            OnLeave(who);
        }

        public void Finish(bool forced = false)
        {
            status = EventStatus.Done;
            
            var temp = members.ToArray();
                    
            foreach (var m in temp)
            {
                if (m.TaskQueue.Current is EventTask e && e.ID == id) m.TaskQueue.Cancel();
                
                Leave(m);
            }
                    
            GameManager.Instance.ClearValue(ID);

            if (!forced && zombie) status = EventStatus.Ready;
            
            OnDone();
        }
        
        protected string GetVarName(string name) => $"{ID}|{name}";

        public float GetVar(string name) => GameManager.Instance.GetVar(GetVarName(name));
        public void SetVar(string name, float val) => GameManager.Instance.SetVar(GetVarName(name), val);

        public abstract bool Equals(Event other);

        public virtual bool Equals(EventBase data)
        {
            return false;
        }
    }
}