using System;
using System.Collections.Generic;
using Game.Map;
using Game.Object;
using Game.Object.Character;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using Game.School;
using UnityEditor;
#endif

namespace Game.Room
{
    public class RoomManager : Singleton<RoomManager>
    {
#if UNITY_EDITOR
        private const string DataPath = "Assets/Resources/Rooms/RoomData.asset";
#endif
        public Action<int> OnRoomLoad = (_) => { };

        [SerializeField] public List<RoomData> roomDatas = new();
        [SerializeField] public int currentRoomIndex;

        [ShowInInspector]
        public RoomData CurrentRoom
        {
            get
            {
                if (roomDatas == null) return null;
                if (!(currentRoomIndex >= 0 && currentRoomIndex < roomDatas.Count)) return null;

                return roomDatas[currentRoomIndex];
            }
        }

        private void Awake()
        {
            GameManager.Instance.OnSetPlayer += OnSetPlayer;
        }

        private void OnSetPlayer(Character player)
        {
            if (player == null) return;

            var z = player.ZIndex;
            currentRoomIndex = z;
            LoadRoomData(CurrentRoom, currentRoomIndex);
        }

        public void Init()
        {
            for (var i = 0;i < roomDatas.Count;i++)
            {
                LoadRoomData(roomDatas[i], i);
            }

            LoadRoomData(CurrentRoom, currentRoomIndex);
        }

        private void LateUpdate()
        {
            if (currentRoomIndex != GameManager.Instance.Player.ZIndex)
            {
                LoadPlayerRoom();
            }
        }

        private void LoadPlayerRoom()
        {
            var z = GameManager.Instance.Player.CPosition.z;
            if (currentRoomIndex == z && 0 <= z && z < roomDatas.Count)  return;
            
            currentRoomIndex = z;
            LoadRoomData(CurrentRoom, currentRoomIndex);
        }
        
        public void LoadRoomData(RoomData data, int idx)
        {
            ClearMap();

            currentRoomIndex = idx;

            data.zIndex = idx;
            
            foreach (var rect in data.rects) rect.FillRect();
            for(int i = 0;i < data.objects.Length;i++) data.objects[i].ZIndex = idx;
            foreach (var jp in data.portals) jp.ZIndex = idx;

            OnRoomLoad(currentRoomIndex);
            
            NavManager.Instance.SetWalkableCell();
        }

        private void ClearMap()
        {
            MapController.Instance.Clear();
        }
        
        public int FindRoomDataIndex(RoomData data)
        {
            return roomDatas.FindIndex(d => d == data);
        }
        
        
        #if UNITY_EDITOR
        
        [Button("save")]
        public void Save()
        {
            GameManager.Instance.InitOnEditorMode(false);
            SaveMapDataOnEditor();
        }

        private void SaveMapDataOnEditor(string path = DataPath, bool replace = false)
        {
            var rData = ScriptableObject.CreateInstance<RoomData>();
            var maps = MapController.Instance.maps;
            var bounds = MapController.Instance.Bounds;
            var jp = new List<Portal>();

            List<RectFilledTile> rects = new();

            if (!replace && AssetDatabase.AssetPathExists(path))
            {
                var cnt = 1;
                path = $"Assets/Resources/Rooms/RoomData{cnt}.asset";

                while (AssetDatabase.AssetPathExists(path))
                {
                    cnt++;
                    path = $"Assets/Resources/Rooms/RoomData{cnt}.asset";
                }
            }

            AssetDatabase.CreateAsset(rData, path);
            AssetDatabase.SaveAssets();

            foreach (var map in maps.Values)
            {
                List<(Vector3Int, TileBase)> tiles = new();
                foreach (var p in bounds.allPositionsWithin)
                {
                    if (map.HasTile(p)) tiles.Add((p, map.GetTileAt(p)));
                }

                var r = GenerateRects(map, tiles);

                if (map.mapID == MapController.Instance.jumpMap.mapID)
                {
                    foreach (var rect in r)
                    {
                        var dots = new List<Vector3Int>();
                        foreach (var dot in rect.rect.allPositionsWithin)
                        {
                            var j = ScriptableObject.CreateInstance<Portal>();
                            
                            j.Init();
                            j.name = j.ID;
                            j.position = new Vector3Int(dot.x, dot.y, currentRoomIndex);

                            AssetDatabase.AddObjectToAsset(j, rData);

                            jp.Add(j);
                        }
                    }
                }


                rects.AddRange(r);
            }

            rData.rects = rects.ToArray();
            rData.portals = jp.ToArray();

            if (!replace)
            {
                roomDatas.Add(rData);
                currentRoomIndex = roomDatas.Count - 1;
            }
            else
            {
                rData.objects = CurrentRoom.objects;
                roomDatas[currentRoomIndex] = rData;
            }

            GameManager.Instance.InitOnEditorMode(false);

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = rData;
            
            AssetDatabase.SaveAssets();
        }

        [Button("edit")]
        public void EditMapDataOnEditor()
        {
            GameManager.Instance.InitOnEditorMode(false);
            var path = AssetDatabase.GetAssetPath(Instance.CurrentRoom);
            
            SaveMapDataOnEditor(path, true);
        }

        [Button("load")]
        public void LoadMapDataOnEditor()
        {
            GameManager.Instance.InitOnEditorMode(true);

            LoadRoomData(CurrentRoom, currentRoomIndex);
            ObjectManager.Instance.Init();
            SchoolManager.Instance.Init();
        }

        private List<RectFilledTile> GenerateRects(Map.Map map, List<(Vector3Int, TileBase)> tiles)
        {
            var size = MapController.Instance.Bounds.size;
            var min = MapController.Instance.Bounds.min;
            var max = MapController.Instance.Bounds.max;
            Func<int, int, (int, int)> fn = (x, y) => (x - min.x, y - min.y);

            var m = math.max(size.x, size.y);
            var X = 1;

            while (X < m) X <<= 1;

            var tileArray = new TileBase[X, X];

            for (var x = min.x; x < max.x; x++)
            for (var y = min.y; y < max.y; y++)
            {
                var (r, c) = fn(x, y);
                var tile = map.GetTileAt(new Vector3Int(x, y));

                tileArray[r, c] = tile;
            }

            var (_, rects) = GenQuadTree(map, tileArray, 0, 0, X);

            return rects;
        }

        private (TileBase, List<RectFilledTile>) GenQuadTree(Map.Map map, TileBase[,] array, int r, int c, int size)
        {
            var min = MapController.Instance.Bounds.min;
            Func<int, int, (int, int)> fn = (x, y) => (x + min.x, y + min.y);
            var (x, y) = fn(r, c);

            if (size == 1)
            {
                var tile = array[r, c];
                if (tile == null) return (null, new List<RectFilledTile>());

                return (array[r, c], new List<RectFilledTile>
                {
                    new()
                    {
                        targetMapID = map.mapID,
                        tile = tile,
                        rect = new RectInt(x, y, 1, 1)
                    }
                });
            }

            var _s = size / 2;
            var _a = GenQuadTree(map, array, r, c, _s);
            var _b = GenQuadTree(map, array, r + _s, c, _s);
            var _c = GenQuadTree(map, array, r, c + _s, _s);
            var _d = GenQuadTree(map, array, r + _s, c + _s, _s);


            if (_a.Item1 != null && _a.Item1 == _b.Item1 && _b.Item1 == _c.Item1 && _c.Item1 == _d.Item1)
                return (array[r, c], new List<RectFilledTile>
                {
                    new()
                    {
                        targetMapID = map.mapID,
                        tile = _a.Item1,
                        rect = new RectInt(x, y, size, size)
                    }
                });

            List<RectFilledTile> ret = new();

            ret.AddRange(_a.Item2);
            ret.AddRange(_b.Item2);
            ret.AddRange(_c.Item2);
            ret.AddRange(_d.Item2);

            return (null, ret);
        }

        [Button("Clear")]
        private void Clear()
        {
            GameManager.Instance.InitOnEditorMode(true);
            
        }

        [Button("Find room data")]
        private void FindRoomData()
        {
            roomDatas.Clear();

            var assets = AssetDatabase.FindAssets("t:RoomData");
            foreach (var guid in assets)
            {
                var data = AssetDatabase.LoadAssetByGUID<RoomData>(new GUID(guid));

                roomDatas.Add(data);
            }
        }
        #endif
    }
}