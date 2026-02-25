using System;
using System.Collections.Generic;
using System.Linq;
using Game.Map;
using Game.Object.Character;
using Game.Room;
using Game.Task;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Action = Game.Task.Action;
using ContextMenu = Game.UI.ContextMenu;

namespace Game.Object
{
    public class ObjectManager : Singleton<ObjectManager>
    {
        [ShowInInspector] private List<IInteractable> _objects = new();

        [SerializeField] private MapObject orig;
        [SerializeField] private MapObject[] mapObjs;
        [SerializeField] public Vector3Int[] occupied;

        public List<IInteractable> Objects => _objects;
        public Character.Character[] Characters => _objects.Where(x=>x is Character.Character).Cast<Character.Character>().ToArray();

        private void Awake()
        {
            Clear();
            Init();
            
            RoomManager.Instance.OnRoomLoad += ActiveObjectsAtZIndex;
        }

        private void Update()
        {
            foreach (var obj in _objects)
            {
                obj.OnUpdate();
            }
        }

        private void ActiveObjectsAtZIndex(int idx)
        {
            foreach (var obj in _objects)
            {
                switch (obj)
                {
                    case Character.Character ch :
                        ch.gameObject.SetActive(ch.ZIndex == idx);
                        break;
                    case MapObject o :
                        o.gameObject.SetActive(o.zIndex == idx);
                        break;
                }
            }
        }

        public void Clear()
        {
            transform.PurgeChild();
            _objects.Clear();
            
            foreach (var obj in _objects)
            {
                if (obj is Character.Character ch) ch.gameObject.SmartDestroy();
            }
            
            mapObjs = Array.Empty<MapObject>();
        }
        
        public void Init()
        {
            Clear();
            
            var mapO = new List<MapObject>();
            for(int i = 0;i < RoomManager.Instance.roomDatas.Count;i++)
            {
                var room = RoomManager.Instance.roomDatas[i];
                foreach (var obj in room.objects ?? Array.Empty<MapObjectInit>())
                {
                    if (obj.objectPrefab == null) continue;
                    
                    var o = Instantiate(obj.objectPrefab, transform);
                    o.Orig = obj.position;
                    o.zIndex = i;
                    o.Init();
                    
                    o.gameObject.SetActive(false);
                    mapO.Add(o);
                    _objects.Add(o);
                }

                mapObjs = mapO.ToArray();
                
                foreach (var jp in room.portals)
                {
                    _objects.Add(jp);
                }
            }
            
            foreach(var o in _objects) o?.Init();

            occupied = mapObjs.SelectMany(o => o.CPositions).ToArray();
            ActiveObjectsAtZIndex(RoomManager.Instance.currentRoomIndex);
        }

        public void Add(IInteractable obj)
        {
            var c = obj as UnityEngine.Component;

            if (c == null) return;
            
            c.transform.SetParent(transform);
            _objects.Add(obj);
        }

        public void Remove(IInteractable obj)
        {
            _objects.Remove(obj);
            var c = obj as UnityEngine.Component;

            if (c == null) return;
            
            Destroy(c.gameObject);
        }
        
        public IInteractable Find(string id)
        {
            foreach (var obj in _objects)
            {
                if (obj == null) continue;
                if (obj.ID == id) return obj;
            }

            return null;
        }

        public IInteractable[] GetObjectAt(Vector3Int cpos)
        {
            List<IInteractable> res = new();
            foreach (var obj in _objects)
            {
                foreach (var c in obj.CPositions)
                {
                    if (cpos == c)
                    {
                        res.Add(obj);
                    }
                }
            }

            return res.ToArray();
        }
    }
}