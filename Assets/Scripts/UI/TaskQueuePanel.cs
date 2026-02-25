using System.Collections.Generic;
using Game.Task;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

namespace Game.UI
{
    public class TaskQueuePanel : UIBehaviour
    {
        [SerializeField] private TaskInfo infoInfoPrefab;
        private ObjectPool<TaskInfo> _objectPool;
        private TaskQueue Queue => GameManager.Instance?.Player?.TaskQueue;

        protected override void Awake()
        {
            base.Awake();

            _objectPool = new ObjectPool<TaskInfo>(
                createFunc: () => Instantiate(infoInfoPrefab, GameManager.TEMP),
                actionOnGet: x => { x.gameObject.SetActive(true); x.transform.SetParent(transform); },
                actionOnRelease: x => { x.gameObject.SetActive(false); x.transform.SetParent(GameManager.TEMP); },
                actionOnDestroy: x => Destroy(x.gameObject),
                collectionCheck: true,
                defaultCapacity: 16,
                maxSize: 100
            );

            GameManager.Instance.OnSetPlayer += (c) =>
            {
                gameObject.SetActive(c != null);
            };
        }

        private void LateUpdate()
        {
            var list = Queue?.List;
            if (list == null) return;

            var l = new List<TaskInfo>();
            foreach (Transform obj in transform)
            {
                l.Add(obj.GetComponent<TaskInfo>());
            }

            foreach (var v in l)
            {
                _objectPool.Release(v);
            }

            if (Queue?.Current != null)
            {
                var curInfo = _objectPool.Get();
                curInfo.Init(Queue.Current);
            }
            foreach (var task in list)
            {
                var child = _objectPool.Get();
                child.Init(task);
            }
        }
    }
}