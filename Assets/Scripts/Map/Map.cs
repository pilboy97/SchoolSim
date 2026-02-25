using System;
using System.Collections.Generic;
// using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Map
{
    [RequireComponent(typeof(Tilemap))]
    public class Map : MonoBehaviour
    {
        [SerializeField] public string mapID;
        [SerializeField] public int layer;
        private Tilemap _tilemap;

        public Action<Vector3Int> OnClickCPosHandler = _ => { };
        public BoundsInt Bounds => _tilemap.cellBounds;

        private void Awake()
        {
            _tilemap = GetComponent<Tilemap>();
        }

        public Vector2 CellToWorld(Vector3Int cPos)
        {
            return _tilemap.layoutGrid.GetCellCenterWorld(new Vector3Int(cPos.x, cPos.y, 0));
        }

        public Vector3Int WorldToCell(Vector2 pos)
        {
            return _tilemap.layoutGrid.WorldToCell(new Vector3(pos.x, pos.y, 0));
        }

        public bool HasTile(Vector3Int cPos)
        {
            return _tilemap.HasTile(cPos);
        }

        public void OnClickCPos(Vector3Int cPos)
        {
            if (!HasTile(cPos)) return;

            OnClickCPosHandler(cPos);
        }

        public void Init(bool clearMap = true)
        {
            _tilemap = GetComponent<Tilemap>();
            if (clearMap) Clear();
        }

        public Vector3Int[] Cells()
        {
            var allTiles = _tilemap.GetTilesBlock(Bounds);

            List<Vector3Int> cPosList = new();

            for (var i = 0; i < Bounds.size.x; i++)
            for (var j = 0; j < Bounds.size.y; j++)
            {
                var tile = allTiles[i + j * Bounds.size.x];
                if (tile != null)
                {
                    var idx = new Vector3Int(i, j);

                    cPosList.Add(Bounds.min + idx);
                }
            }

            return cPosList.ToArray();
        }

        public TileBase GetTileAt(Vector3Int cPos)
        {
            return _tilemap.GetTile(new Vector3Int(cPos.x, cPos.y, 0));
        }

        public Vector3Int WorldToCell(Vector3 pos)
        {
            var p = _tilemap.layoutGrid.WorldToCell(pos);

            return new Vector3Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y));
        }

        public void SetTiles(TileBase tile, RectInt rect)
        {
            var area = rect.size.x * rect.size.y;
            var tiles = new TileBase[area];
            for (int i = 0; i < area; i++)
            {
                tiles[i] = tile;
            }

            var position = new Vector3Int(rect.xMin, rect.yMin, 0);
            var size = new Vector3Int(rect.width, rect.height, 1);

            var bounds = new BoundsInt(position, size);

            _tilemap.SetTilesBlock(bounds, tiles);
        }

        public void Clear()
        {
            _tilemap.ClearAllTiles();
        }

        public Vector3Int LocalCPos(Vector3Int cpos)
        {
            return cpos - Bounds.min;
        }

        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Awake();
            
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(Vector3.zero, 0.5f); 
            
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(CellToWorld(Vector3Int.zero), 0.5f); 
        }
        #endif
    }
}