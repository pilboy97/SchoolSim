using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Map
{
    [RequireComponent(typeof(Grid))]
    public class MapController : Singleton<MapController>
    {
        [SerializeField] public Map groundMap;
        [SerializeField] public Map wallMap;
        [SerializeField] public Map jumpMap;
        
        [SerializeField] private Grid grid;

        public Dictionary<string, Map> maps;

        public BoundsInt Bounds
        {
            get
            {
                // var bounds = maps.Values.Select(m => m.Bounds).ToArray();
                var bounds = new List<BoundsInt>(maps.Values.Count);
                foreach (var bound in maps.Values)
                {
                    bounds.Add(bound.Bounds);
                }
                
                var (minx, miny) = (bounds[0].xMin, bounds[0].yMin);
                var (maxx, maxy) = (bounds[0].xMax, bounds[0].yMax);

                foreach (var b in bounds)
                {
                    minx = math.min(minx, b.xMin);
                    miny = math.min(miny, b.yMin);
                    maxx = math.max(maxx, b.xMax);
                    maxy = math.max(maxy, b.yMax);
                }

                var min = new Vector3Int(minx, miny);
                var max = new Vector3Int(maxx, maxy);

                var pos = min;
                var size = max - min;

                return new BoundsInt(new Vector3Int(pos.x, pos.y, 0), new Vector3Int(size.x, size.y, 1));
            }
        }
        
        public Vector3Int WorldToCell(Vector3 pos)
        {
            var p = grid.WorldToCell(pos);
            p.z = (int)pos.z;

            return p;
        }

        public Vector3 CellToWorld(Vector3Int cPos)
        {
            var ret = grid.GetCellCenterWorld(cPos);
            ret.z = cPos.z;

            return ret;
        }

        public bool HasTile(Vector3Int cPos)
        {
            foreach (var tilemap in maps.Values)
                if (tilemap.HasTile(cPos))
                    return true;

            return false;
        }

        public void Init()
        {
            Init(true);
        }
        
        public void Init(bool clearMap)
        {
            InputManager.Instance.OnClickHandler += OnClick;

            maps = new Dictionary<string, Map>();
            maps[groundMap.mapID] = groundMap;
            maps[wallMap.mapID] = wallMap;
            maps[jumpMap.mapID] = jumpMap;
            
            foreach (var map in maps.Values) map.Init(clearMap);
        }
        
        public Map[] GetTileMapAtCell(Vector3Int cPos)
        {
            List<Map> list = new();

            foreach (var tilemap in maps.Values)
                if (tilemap.HasTile(cPos))
                    list.Add(tilemap);

            return list.ToArray();
        }

        public void OnClick(Vector2 wPos)
        {
            var cPos = WorldToCell(wPos);
            if (!HasTile(cPos)) return;

            foreach (var map in maps.Values.OrderBy(m=>-m.layer))
            {
                if (map.HasTile(cPos))
                {
                    map.OnClickCPos(cPos);
                    return;
                }
            }
        }

        public void Clear()
        {
            foreach (var map in maps.Values) map?.Clear();
        }

        public Vector3Int LocalCPos(Vector3Int cpos)
        {
            return cpos - (Vector3Int)Bounds.min;
        }
    }
}