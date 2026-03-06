using System;
using System.Collections.Generic; 
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Object;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Task
{
    [RequireComponent(typeof(Object.Character.Character))]
    public class TaskQueue : MonoBehaviour
    {
        public System.Action OnUpdateQueue = () => { };
        private Object.Character.Character owner;

        private CancellationTokenSource _base;
        private CancellationTokenSource _cts;

        public List<ITask> List => list;
        private List<ITask> list = new();
        public bool Empty => list.Count == 0;
        public ITask Front => list.FirstOrDefault();
        public ITask Back => list.LastOrDefault();
        
        #if UNITY_EDITOR
        [ShowInInspector] private string FrontDesc => Front?.Desc;
        #endif

        public ITask Current;

        private void Awake()
        {
            owner = GetComponent<Object.Character.Character>();
            
            _base = new CancellationTokenSource();
            _base.RegisterRaiseCancelOnDestroy(this);

            ExecuteQueueAsync().Forget();

            OnUpdateQueue();
        }

        private async UniTaskVoid ExecuteQueueAsync()
        {
            while (true)
            {
                if (GameManager.IsQuitting || _base.IsCancellationRequested) return;

                if (list.Count == 0)
                {
                    await UniTask.NextFrame();
                    continue;
                }

                Current = PopFront();
                _cts = CancellationTokenSource.CreateLinkedTokenSource(_base.Token);

                try
                {
                    if (Current == null)
                    {
                        await UniTask.NextFrame();
                        continue;
                    }

                    owner.Busy = Current.Busy;
                    await Current.DoAsync(_cts.Token).SuppressCancellationThrow();
                }
                finally
                {
                    var tempCts = _cts;
                    _cts = null;        
    
                    tempCts?.Dispose(); 
                }

                Current = null;
                owner.Busy = false;
            }
        }

        public ITask PushBack(Action action, IInteractable other)
        {
            var ret = new ActionTask(owner, other, action);
            
            list.Add(ret);
            OnUpdateQueue();

            return ret;
        }
        public ITask PushBack(Event.SimpleEvent e, IInteractable other)
        {
            var ret = new EventTask(e, owner);
            
            list.Add(ret);
            OnUpdateQueue();

            return ret;
        }

        public ITask PushFront(Action action, IInteractable other)
        {
            var task = new ActionTask(owner, other, action);
            
            Cancel();
            list.Insert(0, task);
            OnUpdateQueue();

            return task;
        }

        public ITask PushFront(Event.Event e, IInteractable other)
        {
            ITask effectTask = new EventTask(e, owner);
            Cancel();
            list.Insert(0, effectTask);
            OnUpdateQueue();

            return effectTask;
        }

        public ITask PushBack(ITask task)
        {
            list.Add(task);
            OnUpdateQueue();

            return task;
        }

        public ITask PushFront(ITask effectTask)
        {
            Cancel();
            list.Insert(0, effectTask);
            OnUpdateQueue();

            return effectTask;
        }


        public void Cancel(string taskID)
        {
            if (taskID == Current?.ID)
            {
                Cancel();
                return;
            }
            
            var idx = list.FindIndex(t => t.ID == taskID);
            if (!(0 <= idx && idx < list.Count)) return;
            
            list.RemoveAt(idx);
            OnUpdateQueue();
        }

        public ITask PopBack()
        {
            var ret = Back;

            list.RemoveAt(list.Count - 1);
            OnUpdateQueue();

            return ret;
        }

        public ITask PopFront()
        {
            if (list.Count == 0) return null;
            
            Cancel();
            var ret = Front;
            list.RemoveAt(0);
            OnUpdateQueue();

            return ret;
        }

        public void Clear()
        {
            Cancel();
            list.Clear();

            OnUpdateQueue();
        }

        public void Cancel()
        {
            try
            {
                if (_cts != null && !_cts.IsCancellationRequested)
                {
                    _cts.Cancel();
                }
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void OnDestroy()
        {
            OnUpdateQueue = () => { };
        }
    }
}