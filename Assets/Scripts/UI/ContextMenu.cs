using System;
using System.Collections.Generic;
using Game.Object;
using Game.Room;
using Game.Time;
using UnityEngine;
using UnityEngine.Pool;
using Action = Game.Task.Action;

namespace Game.UI
{
    public class ContextMenu : Singleton<ContextMenu>
    {
        [Serializable]
        public class Item
        {
            public string name;
            public string desc;
            public Action onClick;
            public IInteractable target;
        }

        [SerializeField] private float timeScale = 1;
        [SerializeField] private Vector3 position;
        [SerializeField] private bool isShow;
        [SerializeField] private float width;
        [SerializeField] private float maxHeight;
        [SerializeField] private Item[] items;
        [SerializeField] private RectTransform content;
        [SerializeField] private ContextMenuItem itemPrefab;

        private Vector3 screenPos => GameManager.Instance.mainCamera.WorldToScreenPoint(position);
        private RectTransform _rectTransform;
        private ObjectPool<ContextMenuItem> _pool;

        public bool IsShow => isShow;

        protected void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _pool = new ObjectPool<ContextMenuItem>(
                createFunc: () => Instantiate(itemPrefab, GameManager.TEMP),
                actionOnGet: x=> { x.transform.SetParent(content);x.gameObject.SetActive(true); },
                actionOnRelease: x => { x.transform.SetParent(GameManager.TEMP); x.gameObject.SetActive(false);}
                );
        }

        public void Init(Vector2 pos, Item[] items)
        {
            this.items = items;
            position = new Vector3(pos.x, pos.y, RoomManager.Instance.currentRoomIndex);

            var active = items != null;

            if (active && !gameObject.activeSelf)
            {
                timeScale = UnityEngine.Time.timeScale;
                TimeManager.Instance.SetTimeScale0_1();
            }
            else if(!active)
            {
                TimeManager.Instance.SetTimeScale(timeScale);
            }
            
            gameObject.SetActive(active);
        }

        private void LateUpdate()
        {
            var list = new List<ContextMenuItem>();
            foreach (Transform t in content.transform)
            {
                list.Add(t.GetComponent<ContextMenuItem>());
            }

            foreach (var x in list)
            {
                _pool.Release(x);
            }

            foreach (var item in items)
            {
                var e = _pool.Get();
                e.Init(this, item, item.target);
            }

            _rectTransform.position = screenPos;
            _rectTransform.sizeDelta = content.rect.height < maxHeight 
                ? new Vector2(width, content.rect.height) 
                : new Vector2(width, maxHeight);
        }
    }
}